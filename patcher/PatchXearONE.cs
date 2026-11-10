using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Threading;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

internal static class PatchXearONE
{
    private static MethodDef FindMethod(TypeDef type, string name)
    {
        return type.Methods.First(m => m.Name == name);
    }

    private static MethodDef AddMethod(TypeDef type, string name, MethodSig sig)
    {
        var method = new MethodDefUser(name, sig, dnlib.DotNet.MethodImplAttributes.IL | dnlib.DotNet.MethodImplAttributes.Managed,
            dnlib.DotNet.MethodAttributes.Private | dnlib.DotNet.MethodAttributes.HideBySig);
        method.Body = new CilBody { InitLocals = true };
        type.Methods.Add(method);
        return method;
    }

    private static void AddBackgroundMethods(ModuleDefMD module, TypeDef mainWindow)
    {
        var voidSig = MethodSig.CreateInstance(module.CorLibTypes.Void);
        var startDeviceDiscovery = AddMethod(mainWindow, "StartDeviceDiscovery", voidSig);
        var worker = AddMethod(mainWindow, "DeviceDiscoveryWorker", voidSig);
        var refresh = AddMethod(mainWindow, "RefreshDeviceUi", voidSig);

        var driverType = module.Find("XearONE.UI.Driver.DriverInstance", true);
        var getInstance = FindMethod(driverType, "getInstance");
        var rebuildAll = FindMethod(driverType, "RebuildAllDeviceList");
        var initHid = FindMethod(mainWindow, "InitHIDDevice");
        var rebuildConsole = FindMethod(mainWindow, "RebuildEndpointConsoleList");
        var refreshCanvas = FindMethod(mainWindow, "RefreshCanvas");
        var refreshSubWindow = FindMethod(mainWindow, "RefreshSubWindow");

        var threadCtor = module.Import(typeof(Thread).GetConstructor(new[] { typeof(ThreadStart) }));
        var threadStartCtor = module.Import(typeof(ThreadStart).GetConstructor(new[] { typeof(object), typeof(IntPtr) }));
        var setIsBackground = module.Import(typeof(Thread).GetProperty("IsBackground").GetSetMethod());
        var threadStart = module.Import(typeof(Thread).GetMethod("Start", Type.EmptyTypes));
        var actionCtor = module.Import(typeof(Action).GetConstructor(new[] { typeof(object), typeof(IntPtr) }));
        // MainWindow inherits DispatcherObject; import the inherited Dispatcher property getter explicitly.
        var dispatcherGetter = module.Import(typeof(System.Windows.Threading.DispatcherObject)
            .GetProperty("Dispatcher", BindingFlags.Instance | BindingFlags.Public).GetGetMethod());
        var dispatcherInvoke = module.Import(typeof(Dispatcher).GetMethod("Invoke", new[] { typeof(Action) }));

        startDeviceDiscovery.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
        startDeviceDiscovery.Body.Instructions.Add(Instruction.Create(OpCodes.Ldftn, worker));
        startDeviceDiscovery.Body.Instructions.Add(Instruction.Create(OpCodes.Newobj, threadStartCtor));
        startDeviceDiscovery.Body.Instructions.Add(Instruction.Create(OpCodes.Newobj, threadCtor));
        startDeviceDiscovery.Body.Instructions.Add(Instruction.Create(OpCodes.Dup));
        startDeviceDiscovery.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4_1));
        startDeviceDiscovery.Body.Instructions.Add(Instruction.Create(OpCodes.Callvirt, setIsBackground));
        startDeviceDiscovery.Body.Instructions.Add(Instruction.Create(OpCodes.Callvirt, threadStart));
        startDeviceDiscovery.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

        var workerDone = Instruction.Create(OpCodes.Nop);
        worker.Body.Instructions.Add(Instruction.Create(OpCodes.Nop));
        worker.Body.Instructions.Add(Instruction.Create(OpCodes.Call, getInstance));
        worker.Body.Instructions.Add(Instruction.Create(OpCodes.Callvirt, rebuildAll));
        worker.Body.Instructions.Add(Instruction.Create(OpCodes.Leave, workerDone));
        var handlerStart = Instruction.Create(OpCodes.Pop);
        worker.Body.Instructions.Add(handlerStart);
        worker.Body.Instructions.Add(Instruction.Create(OpCodes.Leave, workerDone));
        worker.Body.Instructions.Add(workerDone);
        worker.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
        worker.Body.Instructions.Add(Instruction.Create(OpCodes.Call, dispatcherGetter));
        worker.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
        worker.Body.Instructions.Add(Instruction.Create(OpCodes.Ldftn, refresh));
        worker.Body.Instructions.Add(Instruction.Create(OpCodes.Newobj, actionCtor));
        worker.Body.Instructions.Add(Instruction.Create(OpCodes.Callvirt, dispatcherInvoke));
        worker.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        worker.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch)
        {
            CatchType = module.Import(typeof(Exception)),
            TryStart = worker.Body.Instructions[0],
            TryEnd = handlerStart,
            HandlerStart = handlerStart,
            HandlerEnd = workerDone
        });

        refresh.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
        refresh.Body.Instructions.Add(Instruction.Create(OpCodes.Call, initHid));
        refresh.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
        refresh.Body.Instructions.Add(Instruction.Create(OpCodes.Call, rebuildConsole));
        refresh.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
        refresh.Body.Instructions.Add(Instruction.Create(OpCodes.Call, refreshCanvas));
        refresh.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
        refresh.Body.Instructions.Add(Instruction.Create(OpCodes.Call, refreshSubWindow));
        refresh.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

    }

    public static int Main(string[] args)
    {
        try
        {
            if (args.Length != 2)
            {
                Console.Error.WriteLine("Usage: PatchXearONE <input.exe> <output.exe>");
                return 2;
            }

            var input = Path.GetFullPath(args[0]);
            var output = Path.GetFullPath(args[1]);
            var module = ModuleDefMD.Load(input);
            var mainWindow = module.Find("XearONE.UI.MainWindow", true);
            var loaded = FindMethod(mainWindow, "Window_Loaded");
            var originalCall = loaded.Body.Instructions.First(i =>
            {
                if (i.OpCode.Code != Code.Callvirt)
                    return false;
                var md = i.Operand as MethodDef;
                return md != null && md.Name == "RebuildAllDeviceList";
            });

            AddBackgroundMethods(module, mainWindow);
            var start = FindMethod(mainWindow, "StartDeviceDiscovery");
            var receiver = loaded.Body.Instructions[loaded.Body.Instructions.IndexOf(originalCall) - 1];
            receiver.OpCode = OpCodes.Ldarg_0;
            receiver.Operand = null;
            originalCall.OpCode = OpCodes.Call;
            originalCall.Operand = start;

            // Device arrival/removal callbacks used to rebuild the device list synchronously
            // on the WPF dispatcher.  Reuse the same background path so unplugging a device
            // cannot freeze the already-visible window either.
            var arrivalCallback = FindMethod(mainWindow, "AudioEndpointRemovalArrivalCallback");
            arrivalCallback.Body = new CilBody { InitLocals = true };
            arrivalCallback.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            arrivalCallback.Body.Instructions.Add(Instruction.Create(OpCodes.Call, start));
            arrivalCallback.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

            // These three calls used to run immediately after the blocking scan.  Leave the
            // canvas empty until the worker has a consistent device snapshot, then refresh it
            // from RefreshDeviceUi on the WPF dispatcher.
            foreach (var instruction in loaded.Body.Instructions.ToList())
            {
                var called = instruction.Operand as MethodDef;
                if (called == null || (called.Name != "InitHIDDevice" &&
                    called.Name != "RebuildEndpointConsoleList" && called.Name != "RefreshCanvas"))
                    continue;
                var index = loaded.Body.Instructions.IndexOf(instruction);
                if (index > 0)
                {
                    loaded.Body.Instructions[index - 1].OpCode = OpCodes.Nop;
                    loaded.Body.Instructions[index - 1].Operand = null;
                }
                instruction.OpCode = OpCodes.Nop;
                instruction.Operand = null;
            }
            module.Write(output);
            Console.WriteLine("Patched: " + output);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("ERROR TYPE: " + ex.GetType().FullName);
            Console.Error.WriteLine("ERROR MESSAGE: " + ex.Message);
            Console.Error.WriteLine("ERROR STACK: " + ex.StackTrace);
            return 1;
        }
    }
}
