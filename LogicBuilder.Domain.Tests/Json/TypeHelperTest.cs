using LogicBuilder.Domain.Json;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Loader;

namespace LogicBuilder.Domain.Tests.Json
{
    public class TypeHelperTest
    {
        private readonly TypeHelper helper;

        public TypeHelperTest()
        {
            helper = new TypeHelper(new TypeNameHelper());
        }

        [Fact]
        public void GetTypes_WithMissingInMemoryDependency_ThrowsReflectionTypeLoadException()
        {
            // 1. Emit the dependency assembly and its type in memory
            var depName = new AssemblyName("DependencyAssembly");

            // Instantiate the PersistedAssemblyBuilder directly
            var depAssemblyBuilder = new PersistedAssemblyBuilder(depName, typeof(object).Assembly);
            var depModuleBuilder = depAssemblyBuilder.DefineDynamicModule("DependencyModule");

            var interfaceBuilder = depModuleBuilder.DefineType(
                "DependencyNamespace.IMissingInterface",
                TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract
            );
            Type missingInterfaceType = interfaceBuilder.CreateType();

            // 2. Emit the target assembly that implements the dependency interface
            var targetName = new AssemblyName("TargetAssembly");
            var targetAssemblyBuilder = new PersistedAssemblyBuilder(targetName, typeof(object).Assembly);
            var targetModuleBuilder = targetAssemblyBuilder.DefineDynamicModule("TargetModule");

            var classBuilder = targetModuleBuilder.DefineType(
                "TargetNamespace.BrokenClass",
                TypeAttributes.Public | TypeAttributes.Class
            );
            classBuilder.AddInterfaceImplementation(missingInterfaceType);
            classBuilder.CreateType();

            var classBuilder2 = targetModuleBuilder.DefineType(
                "TargetNamespace.UnbrokenClass",
                TypeAttributes.Public | TypeAttributes.Class
            );
            classBuilder2.CreateType();

            // 3. Serialize both dynamic assemblies to raw byte arrays
            using var depStream = new MemoryStream();
            depAssemblyBuilder.Save(depStream);

            using var targetStream = new MemoryStream();
            targetAssemblyBuilder.Save(targetStream);
            byte[] targetBytes = targetStream.ToArray();

            // 4. Load the target assembly into an isolated Context that blocks the dependency
            var isolatedContext = new DependencyBlockingLoadContext(depName.Name!);

            try
            {
                Assembly loadedAssembly = isolatedContext.LoadFromStream(new MemoryStream(targetBytes));

                // act assert
                var result = helper.LoadTypesFromAssembly(loadedAssembly).ToArray();

                Assert.Single(result);
            }
            finally
            {
                // 5. Explicitly initiate unloading the context
                isolatedContext.Unload();
            }
        }

        public class DependencyBlockingLoadContext(string blockedAssemblyName) : AssemblyLoadContext(isCollectible: true)
        {
            private readonly string _blockedAssemblyName = blockedAssemblyName;

            protected override Assembly? Load(AssemblyName assemblyName)
            {
                // Check if the runtime is trying to load the dependency you want to block/override
                if (assemblyName.Name == _blockedAssemblyName)
                {
                    // Throwing directly forces the runtime loader to fail validation for this assembly
                    throw new FileNotFoundException($"Simulated missing dependency: {assemblyName.Name}");
                }

                // Return null to allow the default context to resolve other standard dependencies (like System.Runtime)
                return null;
            }
        }
    }
}
