using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Security.Cryptography;

if (args.Length != 4) throw new ArgumentException("Usage: Patcher <original exe> <runtime dll> <reference directory> <output exe>");
string input = Path.GetFullPath(args[0]), output = Path.GetFullPath(args[3]);
if (input.Equals(output, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Input must remain untouched.");
string hash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(input)));
string[] allowed = ["960A03BFF6050CF7BE16DFC1A7B19E10FC2C4F8F835A6A3B135A50DD9E6BA2F3"];
if (!allowed.Contains(hash)) throw new InvalidOperationException("Unsupported build SHA256: " + hash);
using var resolver = new DefaultAssemblyResolver();
resolver.AddSearchDirectory(Path.GetDirectoryName(input));
resolver.AddSearchDirectory(Path.GetDirectoryName(Path.GetFullPath(args[1])));
foreach (var root in new[] { @"C:\Windows\Microsoft.NET\assembly\GAC_32", @"C:\Windows\Microsoft.NET\assembly\GAC_MSIL" })
    if (Directory.Exists(root)) foreach (var dir in Directory.GetDirectories(root, "Microsoft.Xna.Framework*"))
        foreach (var versionDir in Directory.GetDirectories(dir)) resolver.AddSearchDirectory(versionDir);
resolver.AddSearchDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages", "microsoft.netframework.referenceassemblies.net45", "1.0.3", "build", ".NETFramework", "v4.5"));
resolver.AddSearchDirectory(Path.GetFullPath(args[2]));
using var game = ModuleDefinition.ReadModule(input, new ReaderParameters { AssemblyResolver = resolver });
using var runtime = ModuleDefinition.ReadModule(args[1]);
if (game.Assembly.Name.Name != "Terraria")
    throw new InvalidOperationException("This repository patches only the Windows Terraria client.");
var hooks = runtime.GetType("ExtendedChest.Hooks");
MethodReference Hook(string name) => game.ImportReference(hooks.Methods.Single(m => m.Name == name));
MethodDefinition Method(string type, string name, int? count = null) => game.GetType(type).Methods.Single(m => m.Name == name && (count == null || m.Parameters.Count == count));
// Insert a sequence while preserving its order. Existing branches keep targeting original code.
void Before(MethodDefinition method, Instruction target, params Instruction[] code)
{
    var il = method.Body.GetILProcessor();
    foreach (var instruction in code) il.InsertBefore(target, instruction);
}
void AtStart(MethodDefinition method, params Instruction[] code) => Before(method, method.Body.Instructions[0], code);
// Rewrite ret itself, so branches to a shared return cannot skip the hook.
void AtReturn(MethodDefinition method, Func<Instruction[]> create)
{
    foreach (var ret in method.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray())
    {
        ret.OpCode = OpCodes.Nop;
        var il = method.Body.GetILProcessor();
        Instruction previous = ret;
        foreach (var instruction in create().Append(Instruction.Create(OpCodes.Ret))) { il.InsertAfter(previous, instruction); previous = instruction; }
    }
}
Instruction Call(string name) => Instruction.Create(OpCodes.Call, Hook(name));
foreach (var method in new[] { Method("Terraria.NetMessage", "SendData"), Method("Terraria.MessageBuffer", "GetData") })
{
    var literal = method.Body.Instructions.Single(i => i.OpCode == OpCodes.Ldstr && (string)i.Operand == "Terraria");
    literal.Operand = "Terraria-ExtendedChest-v2-";
}
var countInit = Method("Terraria.ID.ItemID", ".cctor");
var countLiteral = countInit.Body.Instructions.Single(i => i.OpCode == OpCodes.Ldc_I4 && i.Operand is int n && n == 6196);
countLiteral.Operand = 6198;
AtReturn(countInit, () => [Call("SetupItemId")]);
AtReturn(Method("Terraria.ID.ItemID/Sets", ".cctor"), () => [Call("SetupSets")]);
var defaults = Method("Terraria.Item", "SetDefaults", 2);
AtStart(defaults, Instruction.Create(OpCodes.Ldarg_0), Instruction.Create(OpCodes.Ldarg_1), Call("SetDefaults"), Instruction.Create(OpCodes.Brfalse, defaults.Body.Instructions[0]), Instruction.Create(OpCodes.Ret));
AtStart(Method("Terraria.Recipe", "CreateRequiredItemQuickLookups"), Call("SetupRecipe"));
AtReturn(Method("Terraria.Lang", "InitializeLegacyLocalization"), () => [Call("SetupLanguage")]);
var getName = Method("Terraria.Lang", "GetItemName", 1);
var getNameStart = getName.Body.Instructions[0];
AtStart(getName, Instruction.Create(OpCodes.Ldarg_0), Instruction.Create(OpCodes.Ldc_I4, 6196), Instruction.Create(OpCodes.Bne_Un, getNameStart), Call("Tier1Name"), Instruction.Create(OpCodes.Ret));
AtStart(getName, Instruction.Create(OpCodes.Ldarg_0), Instruction.Create(OpCodes.Ldc_I4, 6197), Instruction.Create(OpCodes.Bne_Un, getName.Body.Instructions[0]), Call("Tier2Name"), Instruction.Create(OpCodes.Ret));
AtReturn(Method("Terraria.Chest", "CreateWorldChest"), () => [Call("Created")]);
AtReturn(Method("Terraria.WorldGen", "PlaceChestDirect"), () => [Instruction.Create(OpCodes.Ldarg_0), Instruction.Create(OpCodes.Ldarg_1), Call("Placed")]);
foreach (var name in new[] { "GetItemDrop_Chests", "GetChestIcon" })
{
    var method = Method("Terraria.WorldGen", name, 2);
    var start = method.Body.Instructions[0];
    AtStart(method, Instruction.Create(OpCodes.Ldarg_0), Instruction.Create(OpCodes.Ldc_I4, 52), Instruction.Create(OpCodes.Bne_Un, start), Instruction.Create(OpCodes.Ldarg_1), Instruction.Create(OpCodes.Brtrue, start), Instruction.Create(OpCodes.Ldc_I4, 6196), Instruction.Create(OpCodes.Ret));
    AtStart(method, Instruction.Create(OpCodes.Ldarg_0), Instruction.Create(OpCodes.Ldc_I4, 53), Instruction.Create(OpCodes.Bne_Un, method.Body.Instructions[0]), Instruction.Create(OpCodes.Ldarg_1), Instruction.Create(OpCodes.Brtrue, method.Body.Instructions[0]), Instruction.Create(OpCodes.Ldc_I4, 6197), Instruction.Create(OpCodes.Ret));
}
var mapOption = Method("Terraria.Map.MapHelper", "GetTileBaseOption");
AtReturn(mapOption, () => [Instruction.Create(OpCodes.Ldarg_2), Instruction.Create(OpCodes.Ldarg_3), Instruction.Create(OpCodes.Ldarg, mapOption.Parameters[4]), Call("MapOption")]);
var search = runtime.GetType("ExtendedChest.SearchUI");
var draw = Method("Terraria.UI.ChestUI", "DrawSlots");
AtStart(draw, Instruction.Create(OpCodes.Ldarg_0), Instruction.Create(OpCodes.Call, game.ImportReference(search.Methods.Single(m => m.Name == "Draw"))), Instruction.Create(OpCodes.Brfalse, draw.Body.Instructions[0]), Instruction.Create(OpCodes.Ret));
AtStart(Method("Terraria.Main", "DoUpdate"), Instruction.Create(OpCodes.Call, game.ImportReference(search.Methods.Single(m => m.Name == "Update"))));
var tileDraw = Method("Terraria.GameContent.Drawing.TileDrawing", "GetTileDrawData");
AtReturn(tileDraw, () => [Instruction.Create(OpCodes.Ldarg, tileDraw.Parameters.Single(p => p.Name == "typeCache")), Instruction.Create(OpCodes.Ldarg, tileDraw.Parameters.Single(p => p.Name == "tileFrameX")), Call("TileFrame")]);
var getTileTexture = Method("Terraria.GameContent.Drawing.TileDrawing", "GetTileDrawTexture", 3);
AtReturn(getTileTexture, () => [Instruction.Create(OpCodes.Ldarg, getTileTexture.Parameters.Single(p => p.Name == "tile")), Call("TileTexture")]);

// Packet 32 normally encodes the chest slot as one byte. Patched peers use Int16 so Tier 2 slots 0..999 synchronize.
var sendData = Method("Terraria.NetMessage", "SendData");
var chestLoad = sendData.Body.Instructions.First(i => i.Operand is FieldReference f && f.FullName == "Terraria.Chest[] Terraria.Main::chest");
var sendConversions = sendData.Body.Instructions.SkipWhile(i => i != chestLoad).Take(30).Where(i => i.OpCode == OpCodes.Conv_U1).Take(2).ToArray();
if (sendConversions.Length != 2) throw new InvalidOperationException("Cannot locate packet 32 slot encoding.");
sendConversions[0].OpCode = OpCodes.Conv_I4;
sendConversions[1].OpCode = OpCodes.Conv_I2;
sendConversions[1].Next.Operand = sendData.Body.Instructions.Select(i => i.Operand).OfType<MethodReference>().First(m =>
    m.DeclaringType.FullName == "System.IO.BinaryWriter" && m.Name == "Write" &&
    m.Parameters.Count == 1 && m.Parameters[0].ParameterType.FullName == "System.Int16");

var getData = Method("Terraria.MessageBuffer", "GetData");
var getDataInstructions = getData.Body.Instructions;
var receiveChestLoad = getDataInstructions.First(i =>
{
    if (i.Operand is not FieldReference field || field.FullName != "Terraria.Chest[] Terraria.Main::chest") return false;
    int index = getDataInstructions.IndexOf(i);
    return getDataInstructions.Skip(Math.Max(0, index - 20)).Take(20).Any(previous =>
        previous.OpCode == OpCodes.Ldc_I4 && previous.Operand is int limit && limit == 8000);
});
int receiveChestIndex = getDataInstructions.IndexOf(receiveChestLoad);
var readByte = getDataInstructions.Take(receiveChestIndex).Reverse().Where(i =>
    i.Operand is MethodReference method && method.Name == "ReadByte").Skip(1).First();
readByte.Operand = getDataInstructions.Select(i => i.Operand).OfType<MethodReference>().First(m =>
    m.DeclaringType.FullName == "System.IO.BinaryReader" && m.Name == "ReadInt16");
// Insertions can make existing short branches exceed their signed-byte range.
var longBranches = new Dictionary<OpCode, OpCode> { [OpCodes.Br_S] = OpCodes.Br, [OpCodes.Brfalse_S] = OpCodes.Brfalse, [OpCodes.Brtrue_S] = OpCodes.Brtrue, [OpCodes.Beq_S] = OpCodes.Beq, [OpCodes.Bge_S] = OpCodes.Bge, [OpCodes.Bge_Un_S] = OpCodes.Bge_Un, [OpCodes.Bgt_S] = OpCodes.Bgt, [OpCodes.Bgt_Un_S] = OpCodes.Bgt_Un, [OpCodes.Ble_S] = OpCodes.Ble, [OpCodes.Ble_Un_S] = OpCodes.Ble_Un, [OpCodes.Blt_S] = OpCodes.Blt, [OpCodes.Blt_Un_S] = OpCodes.Blt_Un, [OpCodes.Bne_Un_S] = OpCodes.Bne_Un, [OpCodes.Leave_S] = OpCodes.Leave };
foreach (var type in AllTypes(game.Types)) foreach (var method in type.Methods.Where(m => m.HasBody)) foreach (var instruction in method.Body.Instructions)
    if (longBranches.TryGetValue(instruction.OpCode, out var replacement)) instruction.OpCode = replacement;
Directory.CreateDirectory(Path.GetDirectoryName(output)!);
game.Write(output + ".tmp");
File.Move(output + ".tmp", output, true);
File.Copy(args[1], Path.Combine(Path.GetDirectoryName(output)!, "ExtendedChest.Runtime.dll"), true);
File.WriteAllText(output + ".patch.txt", $"ExtendedChest prototype\nInput SHA256: {hash}\nTier 1 item/style: 6196/52\nTier 2 item/style: 6197/53\n");
Console.WriteLine("Patched copy: " + output);

static IEnumerable<TypeDefinition> AllTypes(IEnumerable<TypeDefinition> types)
{
    foreach (var type in types) { yield return type; foreach (var nested in AllTypes(type.NestedTypes)) yield return nested; }
}
