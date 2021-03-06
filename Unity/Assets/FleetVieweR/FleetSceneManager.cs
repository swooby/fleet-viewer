﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Analytics;

namespace FleetVieweR
{
    public class FleetSceneManager : MonoBehaviour
    {
        public const bool VERBOSE_LOG = false;
        public const bool GL_WIREFRAME = false;

        public const bool RESET_SETTINGS = false;

        private void LoadModels()
        {
            //Debug.Log("LoadModels()");
            if (true)
            {
                //
                // The simplest way to load; usually intended for testing purposes only...
                //

                // Order shortest to longest...
                // ...to give the appearance of loading fast right from the start...
                // ...as opposed to slowing down when loading the smaller ships.

                List<string> modelsToLoad = new List<string>();

                if (true)
                {
                    if (false)
                    {
                        if (true)
                        {
                            //
                            // knight55
                            //

                            modelsToLoad.Add(StarCitizen.Origin_X1_Base);          // \
                            modelsToLoad.Add(StarCitizen.Dragonfly);               // | Race Team Pack
                            modelsToLoad.Add(StarCitizen.Nox);                     // /
                            modelsToLoad.Add(StarCitizen.Origin_X1_Base);          // \
                            modelsToLoad.Add(StarCitizen.Origin_X1_Velocity);      // | X1 Three Pack
                            modelsToLoad.Add(StarCitizen.Origin_X1_Base); // Force // /
                            modelsToLoad.Add(StarCitizen.Origin_X1_Base);          // \
                            modelsToLoad.Add(StarCitizen.Dragonfly);               // | Race Team Pack
                            modelsToLoad.Add(StarCitizen.Nox);                     // /
                            modelsToLoad.Add(StarCitizen.Origin_X1_Base);          // \
                            modelsToLoad.Add(StarCitizen.Dragonfly);               // | Race Team Pack
                            modelsToLoad.Add(StarCitizen.Nox);                     // /
                            modelsToLoad.Add(StarCitizen.Cyclone_TR);              // \
                            modelsToLoad.Add(StarCitizen.Ursa_Rover);              // | Offroad Vehicle Pack
                                                                                   // Greycat                                        // /
                            modelsToLoad.Add(StarCitizen.Cyclone);                 // \
                            modelsToLoad.Add(StarCitizen.Cyclone_AA);              // |
                            modelsToLoad.Add(StarCitizen.Cyclone_RC);              // | Cyclone Five Pack
                            modelsToLoad.Add(StarCitizen.Cyclone_RN);              // |
                            modelsToLoad.Add(StarCitizen.Cyclone_TR);              // /
                            modelsToLoad.Add(StarCitizen.Cyclone);
                            modelsToLoad.Add(StarCitizen.Cyclone);
                            modelsToLoad.Add(StarCitizen.Nox);                     // \
                            modelsToLoad.Add(StarCitizen.Nox);                     // |
                            modelsToLoad.Add(StarCitizen.Nox);                     // | Nox Five Pack
                            modelsToLoad.Add(StarCitizen.Nox);                     // |
                            modelsToLoad.Add(StarCitizen.Nox);                     // /
                            modelsToLoad.Add(StarCitizen.Nox);                     // \
                            modelsToLoad.Add(StarCitizen.Nox);                     // |
                            modelsToLoad.Add(StarCitizen.Nox);                     // | Nox Five Pack
                            modelsToLoad.Add(StarCitizen.Nox);                     // |
                            modelsToLoad.Add(StarCitizen.Nox);                     // /
                            modelsToLoad.Add(StarCitizen.Nox);                     // \
                            modelsToLoad.Add(StarCitizen.Nox);                     // / Nox Two Pack
                            modelsToLoad.Add(StarCitizen.MPUV_Cargo);
                            modelsToLoad.Add(StarCitizen.MPUV_Cargo);
                            modelsToLoad.Add(StarCitizen.MPUV_Cargo);
                            modelsToLoad.Add(StarCitizen.MPUV_Cargo);
                            modelsToLoad.Add(StarCitizen.MPUV_Cargo);
                            modelsToLoad.Add(StarCitizen.MPUV_Cargo);
                            modelsToLoad.Add(StarCitizen.Dragonfly);
                            modelsToLoad.Add(StarCitizen.Dragonfly);
                            modelsToLoad.Add(StarCitizen.Dragonfly);
                            modelsToLoad.Add(StarCitizen.Dragonfly);
                            modelsToLoad.Add(StarCitizen.Dragonfly);
                            modelsToLoad.Add(StarCitizen.Dragonfly);
                            modelsToLoad.Add(StarCitizen.Gladius); // Valiant
                            modelsToLoad.Add(StarCitizen.F7CM_SuperHornet);
                            modelsToLoad.Add(StarCitizen.Gladiator);
                            modelsToLoad.Add(StarCitizen.Prospector);
                            modelsToLoad.Add(StarCitizen.Sabre); // Comet
                            modelsToLoad.Add(StarCitizen.Vanguard_Sentinel); // Warden
                            modelsToLoad.Add(StarCitizen.Idris_P);
                            modelsToLoad.Add(StarCitizen.Retaliator_Base);
                            modelsToLoad.Add(StarCitizen.Origin_325a);
                            modelsToLoad.Add(StarCitizen.Terrapin);
                            modelsToLoad.Add(StarCitizen.Hull_C);
                            modelsToLoad.Add(StarCitizen.Caterpillar);
                            modelsToLoad.Add(StarCitizen.Esperia_Prowler);
                            modelsToLoad.Add(StarCitizen.Caterpillar);
                            modelsToLoad.Add(StarCitizen.Prospector);
                            modelsToLoad.Add(StarCitizen.Carrack); // Loaner: Aquilla
                            modelsToLoad.Add(StarCitizen.Starfarer);
                            //modelsToLoad.Add(StarCitizen.Origin_350r);
                            modelsToLoad.Add(StarCitizen.Origin_85x);
                            modelsToLoad.Add(StarCitizen.Polaris);
                        }
                        else
                        {
                            //
                            // night55
                            //

                            modelsToLoad.Add(StarCitizen.Origin_X1_Base);
                            modelsToLoad.Add(StarCitizen.Dragonfly);
                            modelsToLoad.Add(StarCitizen.Nox);
                            modelsToLoad.Add(StarCitizen.Cyclone_TR);
                            modelsToLoad.Add(StarCitizen.Ursa_Rover);
                            // Greycat
                            modelsToLoad.Add(StarCitizen.Cyclone);
                            modelsToLoad.Add(StarCitizen.Cyclone_AA);
                            modelsToLoad.Add(StarCitizen.Cyclone_RC);
                            modelsToLoad.Add(StarCitizen.Cyclone_RN);
                            modelsToLoad.Add(StarCitizen.Cyclone_TR);
                            modelsToLoad.Add(StarCitizen.Nox);
                            modelsToLoad.Add(StarCitizen.Nox);
                            modelsToLoad.Add(StarCitizen.Nox);
                            modelsToLoad.Add(StarCitizen.Nox);
                            modelsToLoad.Add(StarCitizen.Nox);
                            modelsToLoad.Add(StarCitizen.Dragonfly);
                            modelsToLoad.Add(StarCitizen.Dragonfly);
                            modelsToLoad.Add(StarCitizen.Dragonfly);
                            modelsToLoad.Add(StarCitizen.Dragonfly);
                            modelsToLoad.Add(StarCitizen.MPUV_Cargo);
                            modelsToLoad.Add(StarCitizen.MPUV_Cargo);
                            modelsToLoad.Add(StarCitizen.Razor);
                            modelsToLoad.Add(StarCitizen.Vanguard_Harbinger); // Hoplite
                            modelsToLoad.Add(StarCitizen.Razor);
                            modelsToLoad.Add(StarCitizen.Razor);
                            modelsToLoad.Add(StarCitizen.Razor);
                            modelsToLoad.Add(StarCitizen.Vanguard_Harbinger); // Hoplite
                            modelsToLoad.Add(StarCitizen.Carrack);
                            modelsToLoad.Add(StarCitizen.Terrapin);
                            modelsToLoad.Add(StarCitizen.Dragonfly);
                            modelsToLoad.Add(StarCitizen.F7C_Hornet); // Wildfire
                            modelsToLoad.Add(StarCitizen.Avenger_Stalker); // Titan Renegade
                            modelsToLoad.Add(StarCitizen.Avenger_Stalker); // Warlock
                            modelsToLoad.Add(StarCitizen.Sabre); // Comet
                            modelsToLoad.Add(StarCitizen.Sabre); // Comet
                            modelsToLoad.Add(StarCitizen.Caterpillar);
                            modelsToLoad.Add(StarCitizen.Caterpillar);
                            modelsToLoad.Add(StarCitizen.F7C_Hornet); // Ghost
                            modelsToLoad.Add(StarCitizen.Herald);
                            modelsToLoad.Add(StarCitizen.MPUV_Cargo);
                            modelsToLoad.Add(StarCitizen.MPUV_Personnel);
                            modelsToLoad.Add(StarCitizen.Carrack);
                            modelsToLoad.Add(StarCitizen.Terrapin);
                            modelsToLoad.Add(StarCitizen.Dragonfly);
                            modelsToLoad.Add(StarCitizen.Dragonfly);
                            modelsToLoad.Add(StarCitizen.Starfarer);
                            modelsToLoad.Add(StarCitizen.Starfarer);
                            modelsToLoad.Add(StarCitizen.Caterpillar);
                            modelsToLoad.Add(StarCitizen.F7CM_SuperHornet);
                        }
                    }
                    else
                    {
                        modelsToLoad.Add(StarCitizen.Origin_85x);
                        modelsToLoad.Add(StarCitizen.Nox);
                        modelsToLoad.Add(StarCitizen.Origin_X1);
                        modelsToLoad.Add(StarCitizen.Dragonfly);
                        //modelsToLoad.Add(StarCitizen.MPUV_Personnel);
                        //modelsToLoad.Add(StarCitizen.Terrapin);
                        //modelsToLoad.Add(StarCitizen.Herald);
                        //modelsToLoad.Add(StarCitizen.Constellation_Aquila);
                        //modelsToLoad.Add(StarCitizen.Origin_600i_Touring);
                        //modelsToLoad.Add(StarCitizen.Polaris);
                        //modelsToLoad.Add(StarCitizen.Idris_P);
                        modelsToLoad.Add(StarCitizen.Pioneer);
                    }
                }
                else
                {
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Nox);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Origin_X1_Base);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Dragonfly);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.MPUV_Cargo);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.MPUV_Personnel);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Khartu_Al);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.M50);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Razor);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.P52_Merlin);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.P72_Archimedes);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Origin_85x);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Reliant_Kore);
                    }
                    for (int i = 0; i < 0; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Reliant_Mako);
                    }
                    for (int i = 0; i < 0; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Reliant_Sen);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Reliant_Tana);
                    }
                    /*
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Mustang_Omega);
                    }
                    for (int i = 0; i < 0; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Mustang_Gamma);
                    }
                    for (int i = 0; i < 0; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Mustang_Delta);
                    }
                    for (int i = 0; i < 0; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Mustang_Beta);
                    }
                    */
                    for (int i = 0; i < 0; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Mustang_Alpha);
                    }
                    /*
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Aurora_LN);
                    }
                    for (int i = 0; i < 0; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Aurora_CL);
                    }
                    for (int i = 0; i < 0; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Aurora_MR);
                    }
                    for (int i = 0; i < 0; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Aurora_LX);
                    }
                    */
                    for (int i = 0; i < 0; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Aurora_ES);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Avenger_Stalker);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Terrapin);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Buccaneer);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Gladius);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Eclipse);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Hull_A);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Gladiator);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Hurricane);
                    }
                    /*
                    for (int i = 0; i < 0; i++)
                    {
                        modelsToLoad.Add(StarCitizen.F7CS_Hornet_Ghost);
                    }
                    for (int i = 0; i < 0; i++)
                    {
                        modelsToLoad.Add(StarCitizen.F7CR_Hornet_Tracker);
                    }
                    */
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.F7C_Hornet);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Herald);
                    }
                    for (int i = 0; i < 0; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Origin_300i);
                    }
                    for (int i = 0; i < 0; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Origin_315p);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Origin_325a);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.F7CM_SuperHornet);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Prospector);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Sabre);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Defender);
                    }
                    // As of 2017/08/22, anything beyond here crashes Pixel VR
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Cutlass_Black);
                    }
                    /*
                    for (int i = 0; i < 0; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Cutlass_Red);
                    }
                    for (int i = 0; i < 0; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Cutlass_Blue);
                    }
                    */
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Vanduul_Scythe);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Esperia_Glaive);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Freelancer);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Esperia_Prowler);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Vanguard_Harbinger);
                    }
                    for (int i = 0; i < 0; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Vanguard_Sentinel);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Redeemer);
                    }
                    for (int i = 0; i < 1; i++) // Pixel takes m:s to load to here
                    {
                        modelsToLoad.Add(StarCitizen.Hull_B);
                    }
                    for (int i = 0; i < 0; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Constellation_Taurus);
                    }
                    for (int i = 0; i < 0; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Constellation_Andromeda);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Constellation_Aquila);
                    }
                    for (int i = 0; i < 0; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Constellation_Phoenix);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Origin_600i_Touring);
                    }
                    /*
                    for (int i = 0; i < 0; i++)
                    {
                        modelsToLoad.Add(StarCitizen.600i_Explorer);
                    }
                    */
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Caterpillar);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Retaliator_Base);
                    }
                    for (int i = 0; i < 1; i++) // Pixel takes m:s to load to here
                    {
                        modelsToLoad.Add(StarCitizen.Crucible);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Genesis);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Starfarer);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Hull_C);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Origin_890_Jump);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Carrack);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Polaris);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Reclaimer);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Orion);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Endeavor);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Hull_D);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Idris_P);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        modelsToLoad.Add(StarCitizen.Hull_E);
                    }
                }

                SortModels(modelsToLoad, SortedModels.SortType.Length);

                LoadNextModel(modelsToLoad);

                //SortModels(SortedModels.SortType.Name);
            }
            else
            {
                if (false)
                {
                    foreach (ModelSettings modelSettings in AppSettings.ModelSettings)
                    {
                        AddSavedModel(modelSettings);

                        //go.name = Guid.NewGuid().ToString();
                        // TODO:(pv) Add scale/move/rotate control widget to GameObject...
                        // TODO:(pv) Add [eventually editable] modelName text to GameObject...
                    }
                }
                else
                {
                    GameObject test = Utils.PrimitiveCubeMesh();
                }
            }
        }

        private void SortModels(List<string> modelKeys, SortedModels.SortType sortType)
        {
            //Debug.Log("SortModels(modelKeys:" + Utils.ToString(modelKeys) + ", sortType:" + sortType + ")");

            IComparer<ModelInfo> comparer = SortedModels.getComparer(sortType);

            SortedList<ModelInfo, string> modelInfos = new SortedList<ModelInfo, string>(comparer);

            foreach (string modelKey in modelKeys)
            {
                ModelInfo modelInfo;
                if (!ModelInfos.TryGetValue(modelKey, out modelInfo))
                {
                    Debug.LogWarning("SortModels: ModelInfos does not contain modelKey:" + Utils.ToString(modelKey));
                    continue;
                }
                //Debug.Log("SortModels: modelInfo:" + modelInfo);
                modelInfos.Add(modelInfo, modelKey);
            }

            modelKeys.Clear();
            modelKeys.AddRange(modelInfos.Values);

            //Debug.Log("SortModels: modelKeys:" + Utils.ToString(modelKeys));
        }

        private AppSettings AppSettings;

        private SortedDictionary<string, ModelInfo> ModelInfos = new SortedDictionary<string, ModelInfo>(StringComparer.OrdinalIgnoreCase);


        private GameObject FleetRoot;
        private GameObject FleetPlanes;
        private GameObject FleetModels;
        private GameObject Player;
        private GameObject Respawn;

        private DateTime timeLoadingStarted = DateTime.MinValue;

        private string messageBoxText;

        private void LoadNextModel(List<string> modelsToLoad)
        {
            if (modelsToLoad == null || modelsToLoad.Count == 0)
            {
                string text;

                if (timeLoadingStarted != DateTime.MinValue)
                {
                    DateTime timeLoadingStopped = DateTime.Now;

                    TimeSpan duration = timeLoadingStopped.Subtract(timeLoadingStarted);

                    string durationString = Utils.ToString(duration);

                    text = "Loaded: Took " + durationString;
                    Debug.LogError("LoadNextModel: text == " + Utils.Quote(text));

                    timeLoadingStarted = DateTime.MinValue;
                }
                else
                {
                    text = "Loaded";
                }

                SetMessageBoxText(text, 5.0f);

                return;
            }

            if (timeLoadingStarted == DateTime.MinValue)
            {
                timeLoadingStarted = DateTime.Now;
            }

            string modelToLoad = modelsToLoad[0];
            modelsToLoad.RemoveAt(0);

            SetMessageBoxText("Loading " + modelToLoad);

            AddNewModel(modelToLoad, () =>
            {
                LoadNextModel(modelsToLoad);
            });
        }

        private void SetMessageBoxText(string text, float removeInSeconds = 0)
        {
            StartCoroutine(SetMessageBoxTextCoroutine(text, removeInSeconds));
        }

        IEnumerator SetMessageBoxTextCoroutine(string text, float removeInSeconds = 0)
        {
            messageBoxText = text;
            if (removeInSeconds > 0)
            {
                yield return new WaitForSeconds(removeInSeconds);
                messageBoxText = null;
            }
            yield return null;
        }

        private GUIStyle centeredStyle;

        private void OnGUI()
        {
            if (!string.IsNullOrEmpty(messageBoxText))
            {
                if (centeredStyle == null)
                {
                    centeredStyle = GUI.skin.GetStyle("Box");
                    centeredStyle.alignment = TextAnchor.MiddleCenter;
                    centeredStyle.wordWrap = true;
                }

                GUI.Box(new Rect((Screen.width) / 2f - (Screen.width) / 8f,
                                 (Screen.height) / 2f - (Screen.height) / 8f,
                                 (Screen.width) / 4f,
                                 (Screen.height) / 4f),
                        messageBoxText,
                        centeredStyle);
            }
        }

        void Start()
        {
            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventAppOpen);

            Input.backButtonLeavesApp = true;

            FleetRoot = GameObject.FindGameObjectWithTag("FleetModels");
            FleetPlanes = GameObject.FindGameObjectWithTag("FleetPlanes");
            FleetModels = GameObject.FindGameObjectWithTag("FleetModels");
            Player = GameObject.FindGameObjectWithTag("Player");
            Respawn = GameObject.FindGameObjectWithTag("Respawn");

            if (RESET_SETTINGS)
            {
                AppSettings.Reset();
            }

            AppSettings = AppSettings.Load();

            SystemName = AppSettings.SystemName;

            //RepositionPlayerToViewFleet();
        }

        void Update()
        {
            // Exit when (X) is tapped.
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
        }

        void OnPreRender()
        {
            if (GL_WIREFRAME)
            {
                GL.wireframe = true;
            }
        }

        void OnPostRender()
        {
            if (GL_WIREFRAME)
            {
                GL.wireframe = false;
            }
        }

        private void OnDrawGizmos()
        {
            DebugDecorate();
        }

        private string systemName;

        private string SystemName
        {
            get
            {
                return systemName;
            }
            set
            {
                if (systemName == value)
                {
                    return;
                }

                //Debug.Log("Attempting to set SystemName = " + Utils.Quote(value));
                ConfigInfo.GetSystemInfos((systemInfos) =>
                {
                    SystemInfo systemInfo;
                    systemInfos.TryGetValue(value, out systemInfo);
                    if (systemInfo == null)
                    {
                        Debug.LogError("SystemName: Failed to load system " + Utils.Quote(value));
                        return;
                    }

                    systemName = value;

                    ConfigInfo.GetModelInfos(systemInfo, (modelInfos) =>
                    {
                        ModelInfos = modelInfos;

                        //Debug.Log("LoadModelInfos: ModelInfos.Count == " + ModelInfos.Count);

                        /*
                        GameObject modelInfosDropdown = GameObject.Find("/Player/Main Camera/Overlay Canvas/ModelInfosDropdown");
                        if (modelInfosDropdown == null)
                        {
                            Debug.LogWarning("LoadModelInfos: modelInfosDropdown == null; ignoring");
                            return;
                        }

                        GvrDropdown dropdown = modelInfosDropdown.GetComponent<GvrDropdown>();
                        if (dropdown == null)
                        {
                            Debug.LogWarning("LoadModelInfos: dropdown == null; ignoring");
                            return;
                        }

                        List<Dropdown.OptionData> optionDatas = new List<Dropdown.OptionData>();
                        foreach (ModelInfo modelInfo in ModelInfos.Values)
                        {
                            if (modelInfo == null || modelInfo.ModelPathLocal == null)
                            {
                                continue;
                            }

                            Dropdown.OptionData optionData = new Dropdown.OptionData()
                            {
                                text = modelInfo.Name
                            };
                            optionDatas.Add(optionData);
                        }

                        dropdown.ClearOptions();
                        dropdown.AddOptions(optionDatas);
                        dropdown.value = 0;

                        Debug.Log("LoadModelInfos: dropdown.options.Count == " + dropdown.options.Count);
                        */

                        LoadModels();
                    });
                });
            }
        }

        private delegate void AsyncLoadModelCaller(GameObject model);

        private void LoadModelAsync(string modelKey, AsyncLoadModelCaller caller)
        {
            ModelInfo modelInfo;

            if (!ModelInfos.TryGetValue(modelKey, out modelInfo) || modelInfo == null)
            {
                Debug.LogError("LoadModelAsync: Failed to load modelKey == " + Utils.Quote(modelKey));
                return;
            }

            modelInfo.LoadModelAsync((model) =>
            {
                caller(model);
            });
        }

        private void AddSavedModel(ModelSettings modelSettings, bool repositionPlayerToViewFleet = true)
        {
            string modelKey = modelSettings.Key;

            LoadModelAsync(modelKey, (model) =>
            {

            });
        }

        private void AddNewModel(string modelKey, Action action = null)
        {
            //Debug.Log("AddNewModel(modelKey:" + Utils.Quote(modelKey) +
            //          ", action:" + action + ")");

            LoadModelAsync(modelKey, (model) =>
            {
                if (VERBOSE_LOG)
                {
                    Debug.LogError("AddNewModel: LoadModelAsync completed");
                }

                if (model == null)
                {
                    Debug.LogWarning("AddNewModel: Failed to load modelKey == " + Utils.Quote(modelKey));
                    return;
                }

                Transform modelTransform = model.transform;
                Bounds modelBounds = Utils.CalculateBounds(modelTransform);
                if (VERBOSE_LOG)
                {
                    Debug.LogError("AddNewModel: BEFORE modelBounds == " + Utils.ToString(modelBounds));
                }
                Vector3 modelLocalPosition = modelTransform.localPosition;
                if (VERBOSE_LOG)
                {
                    Debug.LogError("AddNewModel: BEFORE modelLocalPosition == " + modelLocalPosition);
                }

                GameObject fleetModels = FleetModels;
                Transform fleetModelsTransform = fleetModels.transform;
                Bounds fleetModelsBounds = Utils.CalculateBounds(fleetModelsTransform);
                if (VERBOSE_LOG)
                {
                    Debug.LogError("AddNewModel: BEFORE fleetModelsBounds == " + Utils.ToString(fleetModelsBounds));
                }

                modelTransform.SetParent(fleetModelsTransform);

                modelLocalPosition.x = -(fleetModelsBounds.size.x + modelBounds.extents.x);
                if (fleetModelsBounds.size.x > 0)
                {
                    modelLocalPosition.x -= 2;
                }
                if (VERBOSE_LOG)
                {
                    Debug.LogError("AddNewModel: AFTER modelLocalPosition == " + modelLocalPosition);
                }

                modelTransform.localPosition = modelLocalPosition;

                modelBounds = Utils.CalculateBounds(modelTransform);
                if (VERBOSE_LOG)
                {
                    Debug.LogError("AddNewModel: AFTER SetParent modelBounds == " + Utils.ToString(modelBounds));
                }

                FleetPlanesPositionAndScale();

                if (PlayerController.HasNeverMoved)
                {
                    RepositionPlayerToViewFleet();
                }

                // TODO:(pv) Auto-arrange/position according to scale and previously loaded models...

                // Calculate width of all loaded non-positioned models
                // Evenly reposition all loaded models

                // TODO:(pv) Save modelSettings...
                //modelSettings.Position = modelPosition;
                //modelSettings.Rotation = modelRotation;

                if (action != null)
                {
                    action();
                }
            });
        }

        private void FleetPlanesPositionAndScale()
        {
            Debug.Log("FleetPlanesPositionAndScale()");

            GameObject fleetPlanes = FleetPlanes;
            Transform fleetPlanesTransform = fleetPlanes.transform;

            GameObject fleetModels = FleetModels;
            Transform fleetModelsTransform = fleetModels.transform;
            Bounds fleetModelsBounds = Utils.CalculateBounds(fleetModelsTransform);
            //Debug.LogError("FleetPlanesPositionAndScale: fleetModelsBounds == " + Utils.ToString(fleetModelsBounds));

            fleetPlanesTransform.localScale = fleetModelsBounds.size;
            fleetPlanesTransform.localPosition = new Vector3(-fleetModelsBounds.size.x, 0, 0);

            Bounds fleetPlanesBounds = Utils.CalculateBounds(fleetPlanesTransform);
            //Debug.LogError("FleetPlanesPositionAndScale: AFTER localScale fleetPlanesBounds == " + Utils.ToString(fleetPlanesBounds));
        }

        private Vector3 PLAYER_UP = Vector3.up;
        //private Vector3 PLAYER_UP = Vector3.back;

        private void RepositionPlayerToViewFleet()
        {
            Debug.Log("RepositionPlayerToViewFleet()");

            GameObject fleetRoot = FleetRoot;
            Bounds fleetRootBounds = Utils.CalculateBounds(fleetRoot);
            //Debug.LogError("RepositionPlayerToViewFleet: fleetRootBounds == " + Utils.ToString(fleetRootBounds));
            Vector3 fleetRootBoundsSize = fleetRootBounds.size;

            //
            // https://docs.unity3d.com/Manual/FrustumSizeAtDistance.html
            //

            Camera camera = Camera.main;
            float cameraFieldOfViewX = camera.fieldOfView;
            //Debug.LogError("RepositionPlayerToViewFleet: cameraFieldOfViewX == " + cameraFieldOfViewX);
            float cameraAspect = camera.aspect;
            //Debug.LogError("RepositionPlayerToViewFleet: cameraAspect == " + cameraAspect);
            float cameraFieldOfViewY = cameraFieldOfViewX / cameraAspect;
            //Debug.LogError("RepositionPlayerToViewFleet: cameraFieldOfViewY == " + cameraFieldOfViewY);

            float opposite;
            float angle;

            float fleetRootWidth = fleetRootBoundsSize.x;
            float fleetRootHeight = fleetRootBoundsSize.y;
            float fleetRootDepth = fleetRootBoundsSize.z;

            if (PLAYER_UP == Vector3.up)
            {
                if (fleetRootWidth > fleetRootDepth)
                {
                    opposite = fleetRootWidth * 0.5f;
                    angle = cameraFieldOfViewX * 0.5f;
                }
                else
                {
                    opposite = fleetRootHeight * 0.5f;
                    angle = cameraFieldOfViewY * 0.5f;
                }
            }
            else if (PLAYER_UP == Vector3.back)
            {
                if (fleetRootWidth > fleetRootDepth)
                {
                    opposite = fleetRootWidth * 0.5f;
                    angle = cameraFieldOfViewX * 0.5f;
                }
                else
                {
                    opposite = fleetRootDepth * 0.5f;
                    angle = cameraFieldOfViewY * 0.5f;
                }
            }
            else
            {
                throw new ArgumentException("Unsupported PLAYER_UP == " + PLAYER_UP);
            }
            //Debug.LogError("RepositionPlayerToViewFleet: opposite == " + opposite);
            //Debug.LogError("RepositionPlayerToViewFleet: angle == " + angle);

            float adjacent = (float)(opposite / Math.Tan(Mathf.Deg2Rad * angle));
            //Debug.LogError("RepositionPlayerToViewFleet: adjacent == " + adjacent);

            Vector3 playerPosition;
            Quaternion playerRotation;
            Vector3 spawnPosition;
            Quaternion spawnRotation;

            if (PLAYER_UP == Vector3.up)
            {
                playerPosition = new Vector3(fleetRootBounds.center.x,
                                           fleetRootBounds.center.y,
                                           -fleetRootBounds.size.z - adjacent);

                playerRotation = Quaternion.Euler(0, 0, 0);

                spawnPosition = playerPosition;
                spawnPosition.y -= 1;

                spawnRotation = playerRotation;
            }
            else if (PLAYER_UP == Vector3.back)
            {
                playerPosition = new Vector3(fleetRootBounds.center.x,
                                           fleetRootBounds.center.y + adjacent,
                                           fleetRootBounds.center.z);

                playerRotation = Quaternion.Euler(90, 180, 0);

                spawnPosition = playerPosition;
                spawnPosition.z += 1;

                spawnRotation = playerRotation;
            }
            else
            {
                throw new ArgumentException("Unsupported PLAYER_UP == " + PLAYER_UP);
            }
            //Debug.LogError("RepositionPlayerToViewFleet: playerPosition == " + playerPosition);
            //Debug.LogError("RepositionPlayerToViewFleet: playerRotation == " + playerRotation);

            Player.transform.localPosition = playerPosition;
            Player.transform.rotation = playerRotation;

            Respawn.transform.localPosition = spawnPosition;
            Respawn.transform.rotation = spawnRotation;
        }

        private Dictionary<int, Color> debugColors = new Dictionary<int, Color>();

        private void DebugDecorate()
        {
            if (FleetModels != null)
            {
                Transform fleetModelsTransform = FleetModels.transform;
                foreach (Transform fleetModelTransform in fleetModelsTransform)
                {
                    DebugDecorate(fleetModelTransform);
                }
                DebugDecorate(fleetModelsTransform);
            }
        }

        private void DebugDecorate(Transform transform)
        {
            Color savedColor = Gizmos.color;

            int instanceId = transform.GetInstanceID();

            Color color;
            if (!debugColors.TryGetValue(instanceId, out color))
            {
                color = UnityEngine.Random.ColorHSV();

                debugColors[instanceId] = color;
            }
            Gizmos.color = color;

            Bounds bounds = Utils.CalculateBounds(transform);

            Gizmos.DrawWireCube(bounds.center, bounds.size);
            Gizmos.DrawWireSphere(bounds.center, 0.5f);
            Gizmos.DrawWireSphere(bounds.max, 0.5f);
            Gizmos.DrawWireSphere(bounds.min, 0.5f);

            Gizmos.color = savedColor;
        }
    }
}