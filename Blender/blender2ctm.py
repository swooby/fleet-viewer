import bpy
import os
import subprocess
import time


class CtmExporter:
  def __init__(self, blender):
    self.scene = blender.context.scene
    self.blend_file_dir = os.path.dirname(blender.data.filepath)

  def exportAll(self):
    for obj in self.scene.objects:
      self.exportObject(obj)

  def exportName(self, name):
    self.exportObject(self.scene.objects[name])

  def exportObject(self, obj):
    self.selectLayers(obj.layers)
    self.selectObject(obj)
    
    filepath = os.path.join(self.blend_file_dir, obj.name)
    filepath_obj = filepath + ".obj"
    filepath_ctm = filepath + ".ctm"
    
    #
    # https://docs.blender.org/api/current/bpy.ops.export_scene.html#bpy.ops.export_scene.obj
    #
    #bpy.ops.export_scene.obj(filepath=filepath_obj, check_existing=False, use_selection=True)
    
    #
    # http://openctm.sourceforge.net/?page=download
    #
    cmd_args = ["ctmconv", filepath_obj, filepath_ctm, "--method", "MG2", "--level", "6"]
    with subprocess.Popen(cmd_args, stdout=subprocess.PIPE, stderr=subprocess.PIPE) as p:
        output, errors = p.communicate()
    print(output.decode("utf-8"))

  def selectLayers(self, layers):
    for i in range(len(self.scene.layers)):
      self.scene.layers[i] = layers[i]

  def selectObject(self, object):
    for obj in self.scene.objects:
      obj.select = False
    object.select = True

        
def main():
  exporter = CtmExporter(bpy)
  exporter.exportAll()
  

if __name__ == '__main__':
    main()
