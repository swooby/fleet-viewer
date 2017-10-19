filename="$(basename ${BASH_SOURCE[0]})"
~/Applications/blender-2.79-macOS-10.6/blender.app/Contents/MacOS/blender --background --python "${filename%.*}".py -- arg1 arg2 arg3
