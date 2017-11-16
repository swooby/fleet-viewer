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
        private static readonly string TAG = Utils.TAG<ConfigInfo>();

        private const bool VERBOSE_LOG = false;

        private const string FIREBASE_BUCKET = "gs://swooby-fleet-viewer.appspot.com";

        private const string FLEET_VIEWER_SYSTEMS_CSV = "Fleet VieweR - Systems.csv";

        private static FirebaseStorage CloudStorage;

        static ConfigInfo()
        {
            CloudStorage = FirebaseStorage.GetInstance(FIREBASE_BUCKET);
        }

        private ConfigInfo()
        {
        }

        public delegate void GetSystemInfosCallback(SortedDictionary<string, SystemInfo> systemInfos);

        public static void GetSystemInfos(GetSystemInfosCallback callback)
        {
            ConfigInfo.EnsureFileCached(FLEET_VIEWER_SYSTEMS_CSV, (cachedPath) =>
            {
                String text = File.ReadAllText(cachedPath);

                SortedDictionary<string, SystemInfo> systemInfos = new SortedDictionary<string, SystemInfo>(StringComparer.OrdinalIgnoreCase);

                CSVReader.ParseText<SystemInfo>(text, (dictionary) =>
                {
                    SystemInfo system = new SystemInfo(dictionary);
                    systemInfos[system.Name] = system;
                    return null;
                });

                callback(systemInfos);
            });
        }

        public delegate void GetModelInfosCallback(SortedDictionary<string, ModelInfo> modelInfos);

        public static void GetModelInfos(SystemInfo systemInfo, GetModelInfosCallback callback)
        {
            ConfigInfo.EnsureFileCached(systemInfo.ConfigPath, (cachedPath) =>
            {
                String text = File.ReadAllText(cachedPath);

                SortedDictionary<string, ModelInfo> modelInfos = new SortedDictionary<string, ModelInfo>(StringComparer.OrdinalIgnoreCase);

                CSVReader.ParseText<ModelInfo>(text, (dictionary) =>
                {
                    ModelInfo modelInfo = new ModelInfo(dictionary);
                    //Debug.Log(TAG + " GetModelInfos: modelInfo.Name:" + Utils.Quote(modelInfo.Name));
                    modelInfos[modelInfo.Name] = modelInfo;
                    return null;
                });

                callback(modelInfos);
            });
        }

        public delegate void EnsureFileCachedCallback(string cachedPath);

        public static void EnsureFileCached(string filePath, EnsureFileCachedCallback callback)
        {
            if (VERBOSE_LOG)
            {
                Debug.Log(TAG + " EnsureFileCached(filePath:" + Utils.Quote(filePath) + ", ...)");
            }

            // Android: 
            //   MacOS: ~/Library/Application Support/swooby_com/Fleet VieweR
            // Windows:
            string applicationPersistentDataPath = Application.persistentDataPath;
            //Debug.Log(TAG + " EnsureFileCached: applicationPersistentDataPath:" + applicationPersistentDataPath);
            string filePathLocal = Path.Combine(applicationPersistentDataPath, filePath);
            string filePathRemote = FIREBASE_BUCKET + '/' + filePath;

            NetworkReachability internetReachability = Application.internetReachability;
            //Debug.Log(TAG + " EnsureFileCached: internetReachability:" + internetReachability);
            // TODO:(pv) Timeout the below GetMetadataAsync since it does not automatically abort if the internet becomes unreachable after this check
            if (internetReachability == NetworkReachability.NotReachable)
            {
                OnEnsureFileCached("EnsureFileCached: Internet is not reachable; not updating cache", filePathLocal, callback);
                return;
            }

            StorageReference storageReference = CloudStorage.GetReferenceFromUrl(filePathRemote);

            if (!File.Exists(filePathLocal))
            {
                OnEnsureFileCaching("EnsureFileCached: File is not cached", storageReference, filePathRemote, filePathLocal, callback);
                return;
            }

            //Debug.Log(TAG + " EnsureFileCached: +GetMetadataAsync()");
            storageReference.GetMetadataAsync().ContinueWith((taskMetadata) =>
            {
                //Debug.Log(TAG + " EnsureFileCached: -GetMetadataAsync()");

                bool isSuccess = !(taskMetadata.IsFaulted || taskMetadata.IsCanceled);

                DateTime lastUpdatedRemote;
                long sizeBytes;
                if (isSuccess)
                {
                    StorageMetadata remoteMetadata = taskMetadata.Result;
                    lastUpdatedRemote = remoteMetadata.UpdatedTimeMillis;
                    sizeBytes = remoteMetadata.SizeBytes;
                }
                else
                {
                    lastUpdatedRemote = DateTime.MinValue;
                    sizeBytes = -1;
                }

                string message;

                DateTime lastUpdatedLocal;
                // Double check that the file didn't disappear during GetMetadataAsync
                if (File.Exists(filePathLocal))
                {
                    if (isSuccess)
                    {
                        lastUpdatedLocal = File.GetLastWriteTime(filePathLocal);
                        if (lastUpdatedLocal >= lastUpdatedRemote)
                        {
                            OnEnsureFileCached(TAG + " EnsureFileCached: File is cached and up to date",
                                               filePathLocal, callback);
                            return;
                        }
                    }
                    else
                    {
                        OnEnsureFileCached(TAG + " EnsureFileCached: Failed to get date on filePathRemote:" + Utils.Quote(filePathRemote),
                                           filePathLocal, callback);
                        return;
                    }

                    message = "EnsureFileCached: File is cached but out of date";
                }
                else
                {
                    message = "EnsureFileCached: File is not cached";
                }

                OnEnsureFileCaching(message, storageReference, filePathRemote, filePathLocal, callback, sizeBytes);
            });
        }

        private static void OnEnsureFileCaching(string message,
                                                    StorageReference storageReference,
                                                    string filePathRemote,
                                                    string filePathLocal,
                                                    EnsureFileCachedCallback callback,
                                                    long sizeBytes = -1)
        {
            Debug.Log(TAG + " " + message + "; Downloading " + filePathRemote + " to " + filePathLocal);
            GetFileAsync(storageReference, filePathLocal, (ignoreA, ignoreB) =>
            {
                OnEnsureFileCached(null, filePathLocal, callback);
            }, sizeBytes);
        }

        private static void OnEnsureFileCached(string message, string filePathLocal, EnsureFileCachedCallback callback)
        {
            if (VERBOSE_LOG && !String.IsNullOrEmpty(message))
            {
                Debug.Log(message);
            }
            if (!File.Exists(filePathLocal))
            {
                filePathLocal = null;
            }
            callback(filePathLocal);
        }

        public delegate void GetFileAsyncCallback(string filePathRemote, string filePathLocal);

        private static void GetFileAsync(StorageReference storageReference, string filePathLocal, GetFileAsyncCallback callback, long sizeBytes = -1)
        {
            StorageProgress<DownloadState> progressListener = new StorageProgress<DownloadState>((state) =>
            {
                string message = String.Format(TAG + " GetFileAsync: Progress: {0}", state.BytesTransferred);
                if (sizeBytes > 0)
                {
                    message += String.Format(" of {0}", sizeBytes);
                }
                message += " bytes transferred.";
                Debug.Log(message);
            });

            string filePathLocalDirectoryName = Path.GetDirectoryName(filePathLocal);
            if (!Directory.Exists(filePathLocalDirectoryName))
            {
                Directory.CreateDirectory(filePathLocalDirectoryName);
            }

            storageReference
                .GetFileAsync(filePathLocal, progressListener, CancellationToken.None)
                .ContinueWith((taskDownload) =>
            {
                if (taskDownload.IsFaulted)
                {
                    Debug.LogError(TAG + " GetFileAsync: Download error");
                    filePathLocal = null;
                }
                else if (taskDownload.IsCanceled)
                {
                    Debug.LogWarning(TAG + " GetFileAsync: Download canceled");
                    filePathLocal = null;
                }
                else
                {
                    Debug.Log(TAG + " GetFileAsync: Download finished");
                }
                callback(storageReference.Path, filePathLocal);
            });
        }

    }
}