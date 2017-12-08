'''
To use:
>>> import importlib
>>> import SetCameras
>>> importlib.reload(SetCameras)
<module 'SetCameras' from '/Users/pv/Development/GitHub/swooby/fleet-viewer/Star Citizen/Idris_Bridge.blend/SetCameras.py'>

>>> data = SetCameras.default
>>> data = SetCameras.processOrbitVertexIndexCameraVertexAndIncrement(**data)
data:{'camera_sphere_vertex_index': 1, 'orbit_vertex_index': 0}

>>> data = SetCameras.processOrbitVertexIndexCameraVertexAndIncrement(**data)
data:{'camera_sphere_vertex_index': 2, 'orbit_vertex_index': 0}

'''
import bpy
import bmesh
from bpy_extras.object_utils import world_to_camera_view
import math
import mathutils
from mathutils import Euler
from mathutils import Matrix
from mathutils import Quaternion
from mathutils import Vector
from mathutils.geometry import normal


def camera_as_planes(scene, obj):
  """
  Return planes in world-space which represent the camera view bounds.
  """
  camera = obj.data
  # normalize to ignore camera scale
  matrix = obj.matrix_world.normalized()
  frame = [matrix * v for v in camera.view_frame(scene)]
  origin = matrix.to_translation()

  planes = []
  is_persp = (camera.type != 'ORTHO')
  for i in range(4):
    # find the 3rd point to define the planes direction
    if is_persp:
      frame_other = origin
    else:
      frame_other = frame[i] + matrix.col[2].xyz

    n = normal(frame_other, frame[i - 1], frame[i])
    d = -n.dot(frame_other)
    planes.append((n, d))

  if not is_persp:
    # add a 5th plane to ignore objects behind the view
    n = normal(frame[0], frame[1], frame[2])
    d = -n.dot(origin)
    planes.append((n, d))

  return planes

def side_of_plane(p, v):
  return p[0].dot(v) + p[1]

def is_segment_in_planes(p1, p2, planes):
  dp = p2 - p1

  p1_fac = 0.0
  p2_fac = 1.0

  for p in planes:
    div = dp.dot(p[0])
    if div != 0.0:
      t = -side_of_plane(p, p1)
      if div > 0.0:
        # clip p1 lower bounds
        if t >= div:
          return False
        if t > 0.0:
          fac = (t / div)
          p1_fac = max(fac, p1_fac)
          if p1_fac > p2_fac:
            return False
      elif div < 0.0:
        # clip p2 upper bounds
        if t > 0.0:
          return False
        if t > div:
          fac = (t / div)
          p2_fac = min(fac, p2_fac)
          if p1_fac > p2_fac:
            return False

  ## If we want the points
  # p1_clip = p1.lerp(p2, p1_fac)
  # p2_clip = p1.lerp(p2, p2_fac)
  return True

def point_in_object(obj, pt):
  xs = [v[0] for v in obj.bound_box]
  ys = [v[1] for v in obj.bound_box]
  zs = [v[2] for v in obj.bound_box]
  pt = obj.matrix_world.inverted() * pt
  return (min(xs) <= pt.x <= max(xs) and
          min(ys) <= pt.y <= max(ys) and
          min(zs) <= pt.z <= max(zs))

def object_in_planes(obj, planes):
  matrix = obj.matrix_world
  box = [matrix * Vector(v) for v in obj.bound_box]
  for v in box:
    if all(side_of_plane(p, v) < 0.0 for p in planes):
      # one point was in all planes
      return True

  # possible one of our edges intersects
  edges = ((0, 1), (0, 3), (0, 4), (1, 2),
           (1, 5), (2, 3), (2, 6), (3, 7),
           (4, 5), (4, 7), (5, 6), (6, 7))
  return any(is_segment_in_planes(box[e[0]], box[e[1]], planes) for e in edges)

def objects_in_planes(objects, planes, origin):
  """
  Return all objects which are inside (even partially) all planes.
  """
  return [obj for obj in objects
          if point_in_object(obj, origin) or
             object_in_planes(obj, planes)]

def select_objects_in_camera(scene, camera=None):
  if not camera:
    camera = scene.camera
  origin = camera.matrix_world.to_translation()
  planes = camera_as_planes(scene, camera)
  objects_in_view = objects_in_planes(scene.objects, planes, origin)
  for obj in objects_in_view:
    obj.select = True

def look_at(obj, world_point):
  '''
  From https://blender.stackexchange.com/a/5220/47021
  '''
  #print("look_at(obj:%r, world_point:%r)" % (obj, world_point))
  #print("look_at: obj.location:%r" % obj.location)
  obj_world_location = obj.matrix_world.to_translation()
  #print("look_at: obj_world_location:%r" % obj_world_location)
  direction = world_point - obj_world_location
  #print("look_at: direction:%r" % direction)
  rot_quat = direction.to_track_quat('-Z', 'Y')
  #print("look_at: obj.rotation_mode:%r" % obj.rotation_mode)
  rot_euler = rot_quat.to_euler()
  #print("look_at: BEFORE rot_euler:%r" % rot_euler)
  #if yaw:
  #  rot_euler.z += yaw
  #if pitch:
  #  rot_euler.x += pitch
  #print("look_at: AFTER rot_euler:%r" % rot_euler)
  obj.rotation_euler = rot_euler

def getView3dAreaAndRegion(context):
  for area in context.screen.areas:
    if area.type == "VIEW_3D":
      for region in area.regions:
        if region.type == "WINDOW":
          return area, region

def object_as_camera(context, view3dAreaAndRegion, obj):
  if not view3dAreaAndRegion:
    view3dAreaAndRegion = getView3dAreaAndRegion(context)
  view3dArea, view3dRegion = view3dAreaAndRegion
  override = context.copy()
  override['area'] = view3dArea
  override['region'] = view3dRegion
  override['active_object'] = obj
  bpy.ops.view3d.object_as_camera(override)
  return view3dAreaAndRegion

def select_border(context, view3dAreaAndRegion=None, extend=True):
  if not view3dAreaAndRegion:
    view3dAreaAndRegion = getView3dAreaAndRegion(context)
  view3dArea, view3dRegion = view3dAreaAndRegion
  #print("view3dArea.height:%r" % view3dArea.height)
  #print("view3dArea.width:%r" % view3dArea.width)
  override = context.copy()
  override['area'] = view3dArea
  override['region'] = view3dRegion
  bpy.ops.view3d.select_border(override,
                               gesture_mode=3,
                               xmin=0,
                               xmax=view3dArea.width,
                               ymin=0,
                               ymax=view3dArea.height,
                               extend=extend)
  return view3dAreaAndRegion


class SelectBridge:
  def __init__(self):
    self.context = bpy.context

    self.view3dAreaAndRegion = getView3dAreaAndRegion(self.context)

    self.scene = self.context.scene

    self.idris = self.scene.objects["Idris_fv_LOD0"]
    self.table_orbit = self.scene.objects["Table Orbit"]
    self.table_orbit_sphere = self.scene.objects["Table Orbit Icosphere"]
    self.table_orbit_sphere_camera = self.scene.objects["Table Orbit Icosphere Camera"]

  def processOrbitVertexIndex(self, orbit_vertex_index=None, camera_sphere_vertex_index=None):
    #print("processOrbitVertexIndex(orbit_vertex_index:%r, camera_sphere_vertex_index:%r)" % (orbit_vertex_index, camera_sphere_vertex_index))
    if orbit_vertex_index == None:
      #bpy.context.scene.objects.active = idris
      #bpy.ops.object.mode_set(mode="EDIT")
      #bpy.ops.mesh.select_all(action="DESELECT")

      #bpy.types.SpaceView3D.use_occlude_geometry = True

      orbit_vertex_index = 0
      for vertex in self.table_orbit.data.vertices:
        print("orbit_vertex_index:%r" % orbit_vertex_index)
        orbit_vertex_index += 1
        self.processOrbitVertex(vertex)
        break

      return

    vertex = self.table_orbit.data.vertices[orbit_vertex_index]

    if camera_sphere_vertex_index is None:
      lookat = None
    else:
      orbit_position_count = len(self.table_orbit.data.vertices)
      #print("orbit_position_count:%r" % orbit_position_count)

      table_orbit_sphere_data_vertices = self.table_orbit_sphere.data.vertices

      table_orbit_sphere_vertex_count = len(table_orbit_sphere_data_vertices)
      #print("table_orbit_sphere_vertex_count:%r" % table_orbit_sphere_vertex_count)

      if camera_sphere_vertex_index >= table_orbit_sphere_vertex_count:
          camera_sphere_vertex_index = 0

      table_orbit_sphere_vertex = table_orbit_sphere_data_vertices[camera_sphere_vertex_index]

      lookat = self.table_orbit_sphere.matrix_world * Vector(table_orbit_sphere_vertex.co)
      #print("lookat:%r" % lookat)

      camera_sphere_vertex_index += 1

      if camera_sphere_vertex_index >= table_orbit_sphere_vertex_count:
        camera_sphere_vertex_index = 0
        orbit_vertex_index += 1

    self.processOrbitVertex(vertex, lookat)

    data = { "orbit_vertex_index":orbit_vertex_index, "camera_sphere_vertex_index":camera_sphere_vertex_index }
    print("data:%r" % data)
    return data

  def processOrbitVertex(self, vertex, lookat=None):
    if lookat is None:
      lookat = self.table_orbit.location
    #print("processOrbitVertex(vertex, lookat:%r" % lookat)

    self.table_orbit_sphere.location = Vector(vertex.co)

    look_at(self.table_orbit_sphere_camera, lookat)

    object_as_camera(self.context, self.view3dAreaAndRegion, self.table_orbit_sphere_camera)

    if True:
      # NOTE: Select only Idris so that orbit/sphere/etc aren't edited/selected
      bpy.context.scene.objects.active = self.idris
      bpy.ops.object.mode_set(mode="EDIT")
      bpy.ops.mesh.select_mode(type="FACE", action="ENABLE")
      #print("mesh_select_mode:%r" % tuple(self.scene.tool_settings.mesh_select_mode))
      select_border(self.context, self.view3dAreaAndRegion)


default = { "orbit_vertex_index":0, "camera_sphere_vertex_index":0 }

def main(orbit_vertex_index=None):
  SelectBridge().processOrbitVertexIndex(orbit_vertex_index)

def processOrbitVertexIndexCameraVertexAndIncrement(orbit_vertex_index=None, camera_sphere_vertex_index=None):
  return SelectBridge().processOrbitVertexIndex(orbit_vertex_index, camera_sphere_vertex_index)


if __name__ == "__main__":
  main()
