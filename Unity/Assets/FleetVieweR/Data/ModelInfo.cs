﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace FleetVieweR
{
    public class ModelInfo
    {
        private static readonly string TAG = Utils.TAG<ModelInfo>();

        public const bool VERBOSE_LOG = false;

        public const string FIELD_NAME = "Name";
        public const string FIELD_MANUFACTURER = "Manufacturer";
        public const string FIELD_FOCUS = "Focus";
        public const string FIELD_LENGTH = "Length";
        public const string FIELD_BEAM = "Beam";
        public const string FIELD_HEIGHT = "Height";
        public const string FIELD_CARGO = "Cargo";
        public const string FIELD_LAST_CHECKED = "Last Checked";
        public const string FIELD_STORE_URL = "Store URL";
        public const string FIELD_MODEL_PATH_REMOTE = "Model Path Remote";
        public const string FIELD_MODEL_PATH_LOCAL = "Model Path Local";
        public const string FIELD_MODEL_ROTATION = "Model Rotation";

        public string Name { get; private set; }
        public string Manufacturer { get; private set; }
        public string Focus { get; private set; }
        public float LengthMeters { get; private set; }
        public float BeamMeters { get; private set; }
        public float HeightMeters { get; private set; }
        public float CargoCubicMeters { get; private set; }
        public DateTime LastChecked { get; private set; }
        public Uri StoreUrl { get; private set; }
        public Uri ModelPathRemote { get; private set; }
        public string ModelPathLocal { get; private set; }
        public Vector3 ModelRotation { get; private set; }

        public override string ToString()
        {
            return string.Format("[ ModelInfo:" +
                                 " Name={0}" +
                                 ", Manufacturer={1}" +
                                 ", Focus={2}" +
                                 ", LengthMeters={3}" +
                                 ", BeamMeters={4}" +
                                 ", HeightMeters={5}" +
                                 ", CargoCubicMeters={6}" +
                                 ", LastChecked={7}" +
                                 ", StoreUrl={8}" +
                                 ", ModelPathRemote={9}" +
                                 ", ModelPathLocal={10}" +
                                 ", ModelRotation={11}" +
                                 " ]",
                                 Name,
                                 Manufacturer,
                                 Focus,
                                 LengthMeters,
                                 BeamMeters,
                                 HeightMeters,
                                 CargoCubicMeters,
                                 LastChecked,
                                 StoreUrl,
                                 ModelPathRemote,
                                 ModelPathLocal,
                                 ModelRotation);
        }

        public ModelInfo(Dictionary<string, string> dictionary)
        {
            Name = dictionary[FIELD_NAME];

            Manufacturer = dictionary[FIELD_MANUFACTURER];

            Focus = dictionary[FIELD_FOCUS];

            try
            {
                LengthMeters = float.Parse(dictionary[FIELD_LENGTH]);
            }
            catch (FormatException)
            {
                LengthMeters = float.NaN;
            }

            try
            {
                BeamMeters = float.Parse(dictionary[FIELD_BEAM]);
            }
            catch (FormatException)
            {
                BeamMeters = float.NaN;
            }

            try
            {
                HeightMeters = float.Parse(dictionary[FIELD_HEIGHT]);
            }
            catch (FormatException)
            {
                HeightMeters = float.NaN;
            }

            try
            {
                CargoCubicMeters = float.Parse(dictionary[FIELD_CARGO]);
            }
            catch (FormatException)
            {
                CargoCubicMeters = float.NaN;
            }

            try
            {
                LastChecked = DateTime.Parse(dictionary[FIELD_LAST_CHECKED]);
            }
            catch (FormatException)
            {
                LastChecked = DateTime.MinValue;
            }

            try
            {
                StoreUrl = new Uri(dictionary[FIELD_STORE_URL]);
            }
            catch (FormatException)
            {
                StoreUrl = null;
            }

            try
            {
                ModelPathRemote = new Uri(dictionary[FIELD_MODEL_PATH_REMOTE]);
            }
            catch (FormatException)
            {
                ModelPathRemote = null;
            }

            ModelPathLocal = dictionary[FIELD_MODEL_PATH_LOCAL];

            String modelRotation;
            dictionary.TryGetValue(FIELD_MODEL_ROTATION, out modelRotation);
            ModelRotation = Utils.ToVector3(modelRotation);
        }

        private GameObject cachedModel;

        public delegate void LoadModelCallback(GameObject model);

        private List<LoadModelCallback> callbacks = new List<LoadModelCallback>();

        public void LoadModelAsync(LoadModelCallback callback)
        {
            try
            {
                if (VERBOSE_LOG)
                {
                    Debug.Log(TAG + " +LoadModelAsync(callback:" + callback + ")");
                }

                if (cachedModel == null)
                {
                    callbacks.Add(callback);

                    if (callbacks.Count == 1)
                    {
                        if (VERBOSE_LOG)
                        {
                            Debug.LogError(TAG + " LoadModelAsync: Non-Cached Load: ModelFactory.LoadModelAsync(...)");
                        }

                        DateTime timeLoadStart = DateTime.Now;
                        ModelFactory.LoadModelAsync(ModelPathLocal, (model) =>
                        {
                            if (VERBOSE_LOG)
                            {
                                Debug.LogError(TAG + " LoadModelAsync: ModelFactory.LoadModelAsync completed");
                            }

                            DateTime timeLoadStop = DateTime.Now;
                            TimeSpan duration = timeLoadStop.Subtract(timeLoadStart);
                            string durationString = Utils.ToString(duration);
                            if (VERBOSE_LOG)
                            {
                                Debug.Log(TAG + " LoadModelAsync: ModelFactory.LoadModelAsync Took " + durationString);
                            }

                            if (model != null)
                            {
                                model.name = Name;
                            }

                            LoadModelCallback tempCallback;
                            lock (callbacks)
                            {
                                while (callbacks.Count > 0)
                                {
                                    model = OnModelLoaded(model);

                                    tempCallback = callbacks[0];
                                    tempCallback(model);
                                    callbacks.RemoveAt(0);
                                }
                            }
                        });
                    }
                    else
                    {
                        if (VERBOSE_LOG)
                        {
                            Debug.LogError(TAG + " LoadModelAsync: ModelFactory.LoadModelAsync already in progress");
                        }
                    }

                    return;
                }

                if (VERBOSE_LOG)
                {
                    Debug.LogError(TAG + " LoadModelAsync: Cached Load");
                }

                GameObject loadedModel = OnModelLoaded(cachedModel);

                callback(loadedModel);
            }
            finally
            {
                if (VERBOSE_LOG)
                {
                    Debug.Log(TAG + " -LoadModelAsync(callback:" + callback + ")");
                }
            }
        }

        private GameObject OnModelLoaded(GameObject model)
        {
            if (VERBOSE_LOG)
            {
                Debug.Log(TAG + " +OnModelLoaded(model:" + model + ")");
            }

            if (cachedModel == null)
            {
                cachedModel = model;
            }
            else
            {
                model = UnityEngine.Object.Instantiate(cachedModel);

                model.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                model.transform.localScale = Vector3.one;
            }

            if (VERBOSE_LOG)
            {
                Transform transform = model.transform;
                Bounds bounds = Utils.CalculateBounds(transform);
                Debug.LogError(TAG + " ModelLoad: AFTER LOAD transform.position == " + transform.position);
                Debug.LogError(TAG + " ModelLoad: AFTER LOAD transform.rotation == " + transform.rotation);
                Debug.LogError(TAG + " ModelLoad: AFTER LOAD bounds == " + Utils.ToString(bounds));
            }

            Normalize(model);

            if (VERBOSE_LOG)
            {
                Debug.Log(TAG + " -OnModelLoaded(model:" + model + ")");
            }

            return model;
        }

        private GameObject Normalize(GameObject model)
        {
            // 1) Rotate so the length is along Z and bow faces -Z
            Utils.NormalizeRotation(model, ModelRotation);

            // 2) Uniformly scale X/Y/Z so that bounds.size.z is the expected length
            Utils.NormalizeScale(model, Vector3.forward, LengthMeters);

            // 3) Use the scaled length to position the stern at Z == 0
            Utils.NormalizePosition(model);

            // These are affecting the scaling and positioning when loading multiple ships
            //Decorate(model);

            Utils.ActivateLodRenderers(model);

            if (VERBOSE_LOG)
            {
                Debug.LogError(TAG + " Normalize: gameObject.transform.position == " + model.transform.position);
                Debug.LogError(TAG + " Normalize: gameObject.transform.rotation == " + model.transform.rotation);
            }

            return model;
        }

        private static void Decorate(GameObject model)
        {
            //Debug.Log(TAG + " Decorate(model)");

            Transform transform = model.transform;

            Bounds bounds = Utils.CalculateBounds(transform);

            GameObject primitive;
            Transform primitiveTransform;

            primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            primitiveTransform = primitive.transform;
            primitive.name = "position";
            primitiveTransform.position = transform.position;
            primitiveTransform.SetParent(transform);

            primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            primitiveTransform = primitive.transform;
            primitive.name = "center";
            primitiveTransform.position = bounds.center;
            primitiveTransform.SetParent(transform);

            primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            primitiveTransform = primitive.transform;
            primitive.name = "min";
            primitiveTransform.position = bounds.min;
            primitiveTransform.SetParent(transform);

            primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            primitiveTransform = primitive.transform;
            primitive.name = "max";
            primitiveTransform.position = bounds.max;
            primitiveTransform.SetParent(transform);
        }
    }
}