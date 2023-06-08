Test_RimIgnore.Run();
Test_PatchDef.Run();
Test_DynamicPatch.Run();
Test_Harmony.Run();
Test_StaticGenericClass.Run();

var c = Console.ForegroundColor;
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("[=] ALL OK!");
Console.ForegroundColor = c;
