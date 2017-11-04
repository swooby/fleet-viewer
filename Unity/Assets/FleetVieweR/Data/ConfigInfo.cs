using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Firebase.Storage;

namespace FleetVieweR
{
    public class ConfigInfo
    {
        private const string FLEET_VIEWER_GOOGLE_SPREADSHEET_ID = "1ammOh0-6BHe2hYFQ5c1pdYrLlNjVaphd5bDidcdWqRo";
        private const string FLEET_VIEWER_GOOGLE_SPREADSHEET_SHEET_ID_SYSTEMS = "1057971452";
        //private const string FLEET_VIEWER_GOOGLE_SPREADSHEET_SHEET_ID_SYSTEM_STAR_CITIZEN_SHIP_MATRIX_2_0 = "1227853955";

        private const string FIREBASE_BUCKET = "gs://swooby-fleet-viewer.appspot.com";

        private static FirebaseStorage CloudStorage;

        static ConfigInfo()
        {
            CloudStorage = FirebaseStorage.GetInstance(FIREBASE_BUCKET);
        }

        private ConfigInfo()
        {
        }

        public static WWW GetFleetViewerSystemsCsv()
        {
            return GetFleetViewerSpreadsheetSheetCsv(FLEET_VIEWER_GOOGLE_SPREADSHEET_SHEET_ID_SYSTEMS);
        }

        /*
        public static WWW GetFleetViewerSystemStarCitizenShipMatrix2_0Csv()
        {
            return getFleetViewerSpreadsheetSheetCsv(FLEET_VIEWER_GOOGLE_SPREADSHEET_SHEET_ID_SYSTEM_STAR_CITIZEN_SHIP_MATRIX_2_0);
        }
        */

        public static WWW GetFleetViewerSpreadsheetSheetCsv(string sheetId)
        {
            return GetSpreadsheetSheetAsCsv(FLEET_VIEWER_GOOGLE_SPREADSHEET_ID, sheetId);
        }

        private static WWW GetSpreadsheetSheetAsCsv(string spreadsheetId, string sheetId)
        {
            string url = string.Format("https://docs.google.com/spreadsheets/d/{0}/export?gid={1}&format=csv", spreadsheetId, sheetId);
            Debug.Log("GetSpreadsheetSheetAsCsv: url:" + Utils.Quote(url));
            return new WWW(url);
        }

        public delegate void EnsureFileCachedCallback(string cachedPath);

        public static void EnsureFileCached(string filePath, EnsureFileCachedCallback callback)
        {
            Debug.Log("EnsureFileCached(filePath:" + Utils.Quote(filePath) + ", ...)");

            string filePathLocal = Path.Combine(Application.persistentDataPath, filePath);
            string filePathRemote = FIREBASE_BUCKET + '/' + filePath;

            StorageReference storageReference = CloudStorage.GetReferenceFromUrl(filePathRemote);
            storageReference.GetMetadataAsync().ContinueWith(taskMetadata =>
            {
                Debug.Log("EnsureFileCached: GetMetadataAsync completed");
                if (taskMetadata.IsFaulted || taskMetadata.IsCanceled)
                {
                    callback(null);
                    return;
                }

                StorageMetadata remoteMetadata = taskMetadata.Result;

                string message;

                DateTime lastUpdatedLocal;
                if (File.Exists(filePathLocal))
                {
                    lastUpdatedLocal = File.GetLastWriteTime(filePathLocal);

                    DateTime lastUpdatedRemote = remoteMetadata.UpdatedTimeMillis;

                    if (lastUpdatedLocal >= lastUpdatedRemote)
                    {
                        Debug.Log("EnsureFileCached: File is cached and up to date");
                        callback(filePathLocal);
                        return;
                    }

                    message = "EnsureFileCached: File is cached but out of date";
                }
                else
                {
                    message = "EnsureFileCached: File is not cached";

                    string filePathLocalDirectoryName = Path.GetDirectoryName(filePathLocal);
                    if (!Directory.Exists(filePathLocalDirectoryName))
                    {
                        Directory.CreateDirectory(filePathLocalDirectoryName);
                    }
                }
                message += "; Downloading " + filePathRemote + " to " + filePathLocal;
                Debug.Log(message);

                long sizeBytes = remoteMetadata.SizeBytes;

                storageReference
                    .GetFileAsync(filePathLocal, new StorageProgress<DownloadState>(state =>
                    {
                        Debug.Log(String.Format("EnsureFileCached: Progress: {0} of {1} bytes transferred.",
                                                state.BytesTransferred, sizeBytes));
                    }), CancellationToken.None)
                    .ContinueWith(taskDownload =>
                    {
                        if (taskDownload.IsFaulted)
                        {
                            Debug.LogError("Download error");
                            filePathLocal = null;
                        }
                        else if (taskDownload.IsCanceled)
                        {
                            Debug.LogWarning("Download canceled");
                            filePathLocal = null;
                        }
                        else
                        {
                            Debug.Log("Download finished");
                        }
                        callback(filePathLocal);
                    });
            });
        }
    }
}