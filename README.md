# MinecraftModTemplateMaker
Generate and extract template projects from my custom example mod format.

| Name in example mod        | Placeholder in template ZIP |
| -------------------------- | --------------------------- |
| `net.example.examplemodid` | `!@PKG`                     |
| `examplemodid`             | `!@MODID`                   |
| `ExampleModName`           | `!@NAME`                    |
| `Example Display Name`     | `!@DISP`                    |
| `Example Description`      | `!@DESC`                    |

Both file paths and text file contents will be processed.

## TemplateExtractor
Extract template ZIP with given metadata.

## TemplateGenerator
Generate template ZIP with given example mod.

## example-template.zip
A somewhat complex template project based on my [NoCaves](https://github.com/Myitian/NoCaves) mod.

Including features:
- Architectury Loom
- Example of how to support a wide range of Minecraft versions
- Basic in-game configuration based on the Cloth Config API