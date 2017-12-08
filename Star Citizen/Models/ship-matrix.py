#!/usr/bin/env python3

import io
import requests
from cachecontrol import CacheControl
from cachecontrol.caches.file_cache import FileCache

def execute(cmd_args, cwd=None):
  print('execute(%r)' % cmd_args)
  with subprocess.Popen(cmd_args, cwd=cwd, stdout=subprocess.PIPE, stderr=subprocess.PIPE) as p:
    output, errors = p.communicate()
  print(output.decode("utf-8"))
  return p.returncode


def main():
  session = CacheControl(requests.session(), cache=FileCache('.cache'))
  response = session.get('https://robertsspaceindustries.com/ship-matrix')
  response.raise_for_status()
  with io.StringIO(response.text) as buff:
    pass

if __name__ == "__main__":
  main()
