# Relic assignment decoding

Current startup responses place `PlayerRelicDto.equippedOnHeroDefinitionId`
in string field 5. Field 4 was observed with varint values; its meaning has
not been established. Leave it unknown rather than interpreting it as a hero.
The existing mapping strips the `hero.` prefix for `RelicItem.EquippedOnHero`.

## Synthetic regression fixtures

These hex strings encode a `RelicPush` containing one relic (ID 1,
`relic.Abacus`, level 7, and unknown numeric field 4 set to 2).
They contain no account data.

| Case | Hex | Expected assignment |
| --- | --- | --- |
| Equipped | `0a290801120c72656c69632e416261637573180720022a136865726f2e416c6265727445696e737465696e` | `hero.AlbertEinstein` |
| Changed | `0a250801120c72656c69632e416261637573180720022a0f6865726f2e426c61636b6265617264` | `hero.Blackbeard` |
| Absent | `0a140801120c72656c69632e41626163757318072002` | Empty protobuf default; no assignment established |
| Explicit empty | `0a160801120c72656c69632e416261637573180720022a00` | Empty; do not assign to a hero |

From the repository root, decode the equipped fixture with `protoc`:

```sh
python3 -c 'import sys; sys.stdout.buffer.write(bytes.fromhex("0a290801120c72656c69632e416261637573180720022a136865726f2e416c6265727445696e737465696e"))' | protoc --proto_path=src/protos/hoh/startup --decode=RelicPush relic_push.proto
```

The output must contain `equippedOnHeroDefinitionId: "hero.AlbertEinstein"`.
The previous schema prints `5: "hero.AlbertEinstein"` as an unknown field
and leaves the named assignment empty. Repeat with the other fixture strings;
ID, definition and level must remain unchanged. Numeric field 4 must never
become a hero assignment.

Regenerate the committed C# decoder with protoc 29.3:

```sh
protoc --proto_path=src/protos/hoh/startup --csharp_opt=file_extension=.g.cs --csharp_out=src/Inn/InnModels.Hoh/Generated relic_push.proto
dotnet build src/Inn/InnModels.Hoh/InnModels.Hoh.csproj
```

For deployment verification, reload the game and use the existing helper export.
Compare a known equipped relic with the resulting `equippedOnHero` value.
After a player changes the assignment, reload and export again to check freshness.
Keep raw startup captures private; do not attach them to issues or pull requests.
