rm ../../Unity/Assets/Runtime\ Transform\ Gizmos
mkdir -p ../../Unity/Assets/Runtime\ Transform\ Gizmos/Resources
cp -R ~/Development/BitBucket/swooby/runtime_transform_gizmos/Asset/Resources/* ../../Unity/Assets/Runtime\ Transform\ Gizmos/Resources

rm ../../Unity/Assets/Runtime\ Transform\ Gizmos\ GoogleVR
ln -s ~/Development/BitBucket/swooby/runtime_transform_gizmos/GoogleVR/ ../../Unity/Assets/Runtime\ Transform\ Gizmos\ GoogleVR
