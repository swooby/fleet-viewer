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

        public delegate void EnsureFileCachedCallback(bool success);

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
                    callback(false);
                    return;
                }

                StorageMetadata metadata = taskMetadata.Result;

                DateTime lastUpdatedRemote = metadata.UpdatedTimeMillis;
                long sizeBytes = metadata.SizeBytes;

                DateTime lastUpdatedLocal = DateTime.MinValue;
                if (File.Exists(filePathLocal))
                {
                    lastUpdatedLocal = File.GetLastWriteTime(filePathLocal);
                }

                if (lastUpdatedLocal != DateTime.MinValue &&
                    lastUpdatedLocal >= lastUpdatedRemote)
                {
                    callback(true);
                    return;
                }

                Debug.Log("Downloading " + filePathRemote + " to " + filePathLocal);
                Directory.CreateDirectory(Path.GetDirectoryName(filePathLocal));
                storageReference
                    .GetFileAsync(
                        filePathLocal,
                        new Firebase.Storage.StorageProgress<DownloadState>(state =>
                        {
                            Debug.Log(String.Format("Progress: {0} of {1} bytes transferred.",
                                                    state.BytesTransferred, sizeBytes));
                        }), CancellationToken.None)
                    .ContinueWith(taskDownload =>
                    {
                        bool success;
                        if (taskDownload.IsFaulted)
                        {
                            //Debug.LogError("Download error");
                            success = false;
                        }
                        else if (taskDownload.IsCanceled)
                        {
                            //Debug.LogWarning("Download canceled");
                            success = false;
                        }
                        else
                        {
                            //Debug.Log("Download finished");
                            success = true;
                        }
                        callback(success);
                    });
            });
        }
    }
}