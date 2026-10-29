#!/usr/bin/env python3
"""KanaWorks SAKUGA VFX tools (MIT). Run from anywhere: python tools/sakuga.py --help"""
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))

from sakugakit.cli import main  # noqa: E402

if __name__ == '__main__':
    sys.exit(main())
