---
tags: [tervain, art, workshop]
created: 2026-10-26
---

# Art Workshop

Related: [[Artwork]] · [[Biome Artwork]] · [[Biome Integration]] · [[Continue on Another Device]]

This archive preserves useful work from the separate chat scratch folder: the original foundation-tile converters, preview helpers, six referenced source images, early native exports and enlarged previews, and the desert, tundra, jungle, swamp, and water prompt sets. `reference-map.json` maps the bundled source references to the scripts that use them.

The original machine-specific generated-image paths in these archived scripts have been changed to bundled relative references. To rerun a script on Windows, first change directory into this `Art Workshop` folder, then run the relevant `.ps1`. They require PowerShell with `System.Drawing`; output stays in this archive's `output/` directory. The parameterized `make-desert-asset.ps1` also needs its source image, asset/layer names, palette, and optional transparency flag.

These scripts and early exports are historical workshop resources. The authoritative editable game art is in `tervain--shattered-realms/Art-work/`, and the fifteen-biome package is in its `BiomeExpansion/` directory. In particular, the current frozen-cave artwork includes a later seam correction and should not be replaced from an older workshop image.

- `output/*-prompts.md` and `output/tundra-wood-prompt.md`: original art prompts.
- `output/*-set-preview.png`: early biome/style reference sheets.
- `output/*-24.png`: early native-size exports; use the current game art for integration.
- `output/*-preview.png`: enlarged individual-tile references.
- `References/`: bundled input images used by the foundation-tile converters.

The later biome prompts, generated sheets, native ASE/PNG pairs, validation tools, and twenty final in-game captures already live in the main art package. Duplicate scratch copies, stale integration-source snapshots, the local staging helper, and a redundant ZIP are not needed to continue development and are not part of this archive.
