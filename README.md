# paint-label

> Effects (FX) for Direct3D 9 (fx_2_0) reader

---

**paint-label** is a partial port of the Effects reading of [mojoshader](https://github.com/icculus/mojoshader), written in pure C#. It also uses some references from [Ved-s/ShaderDecompiler](https://github.com/Ved-s/ShaderDecompiler), which adapts mojoshader's parsing as well.

The basic Effect (`HlslEffect`) parsing is mostly a faithful recreation of mojoshader's. The shader (`Shader`, `Preshader`, and friends) is lifted directly from ShaderDecompiler with changes to suit this project and handling of additional cases, as well as bug fixes.

Tested against the entire suite of Wrath of the Gods shaders, as well as some hand-written test cases.
