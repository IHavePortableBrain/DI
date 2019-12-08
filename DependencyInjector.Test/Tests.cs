using DependencyInjector.Configuration;
using DependencyInjector.Test.Class;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.CSharp;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Loader;
using System.Collections;

namespace DependencyInjector.Test
{
    [TestClass]
    public class Tests
    {
        private DependencyConfiguration configuration;
        private DI DI;

        [TestInitialize]
        public void TestInitialize()
        {
            configuration = new DependencyConfiguration();
        }

        [TestMethod]
        [ExpectedException(typeof(MissingMethodException))]
        public void NoImplementationForSuchDependencyTest()
        {
            DI = new DI(configuration);
            var actual = DI.Resolve<I>();
        }

        [TestMethod]
        public void SimpleDependencyTest()
        {
            configuration.Register<I, Impl1>();

            DI = new DI(configuration);
            var actual = DI.Resolve<I>();

            Assert.AreEqual(typeof(Impl1), actual.GetType());
        }

        [TestMethod]
        public void AsSelfTest()
        {
            configuration.Register<Impl1, Impl1>();

            DI = new DI(configuration);
            var actual = DI.Resolve<Impl1>();

            Assert.AreEqual(typeof(Impl1), actual.GetType());
        }

        [TestMethod]
        public void NotCompatibleToDependencyImplementationRegistrationTest()
        {
            //configuration.Register<I, NotImpl>(); equavalent

            string codeToCompile = @"
            using System;
            using DependencyInjector.Configuration;
            using DependencyInjector.Test.Class;
            namespace RoslynCompileSample
            {
                public class Writer
                {
                    public void Write(string message)
                    {
                        DependencyConfiguration configuration = new DependencyConfiguration();;
                        configuration.Register<I, NotImpl>();
                    }
                }
            }";

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(codeToCompile);

            string assemblyName = Path.GetRandomFileName();
            var refPaths = new[] {
                typeof(object).GetTypeInfo().Assembly.Location,
                Path.GetDirectoryName(typeof(object).GetTypeInfo().Assembly.Location) + "\\mscorlib.dll",
                typeof(DependencyConfiguration).GetTypeInfo().Assembly.Location,
                typeof(NotImpl).GetTypeInfo().Assembly.Location,
                Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Runtime.dll")
            };
            MetadataReference[] references = refPaths.Select(r => MetadataReference.CreateFromFile(r)).ToArray();

            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

                //result.Diagnostics[0].Info.MessageIdentifier = "CS0311"
                if (result.Success)
                    Assert.Fail("Not compatible to dependency implementation passed registration.");
                if (result.Diagnostics.Count() != 1)
                    Assert.Fail("Unexpected error diagnostic count.");
                if (result.Diagnostics.First().Descriptor.Id != "CS0311")
                    Assert.Fail("Unexpected error diagnostic.");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidConfigurationTest()
        {
            configuration.Register<I, AbstractImpl>();
            DI = new DI(configuration);

            I actual = DI.Resolve<I>();
        }

        [TestMethod]
        public void ManyImplementationsOfOneDependencyResolveTest()
        {
            configuration.Register<I, Impl1>();
            configuration.Register<I, Impl2>();

            DI = new DI(configuration);
            var actual = DI.Resolve<IEnumerable<I>>();

            Assert.AreEqual(typeof(Impl1), actual.First().GetType());
            Assert.AreEqual(typeof(Impl2), actual.Last().GetType());
        }

        [TestMethod]
        public void GenericDependencyTest()
        {
            configuration.Register<IGeneric<ArrayList>, GenericImpl<ArrayList>>();

            DI = new DI(configuration);
            var actual = DI.Resolve<IGeneric<ArrayList>>();

            Assert.AreEqual(typeof(GenericImpl<ArrayList>), actual.GetType());
        }

        [TestMethod]
        public void OpenGenericDependencyTest()
        {
            //configuration.Register<ArrayList, ArrayList>();
            configuration.Register(typeof(IGeneric<>), typeof(GenericImpl<>));

            DI = new DI(configuration);
            var actual = DI.Resolve<IGeneric<IEnumerable>>();

            Assert.AreEqual(typeof(GenericImpl<ArrayList>), actual.GetType());
        }

        [TestMethod]
        public void ValidateGenericTypesCompatabilityWithCovariantTypesIsTrueTest()
        {
            configuration.ValidateRegistration(typeof(IGeneric<>), typeof(GenericImpl<>));
            configuration.ValidateRegistration(typeof(IGeneric<IEnumerable>), typeof(GenericImpl<ArrayList>));
        }

        [TestMethod]
        public void DependencyHasDependenciesTest()
        {
            configuration.Register<I, DependentImpl>();

            DI = new DI(configuration);
            var actual = DI.Resolve<I>();

            Assert.AreEqual(typeof(DependentImpl), actual.GetType());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RecursiveDependenciesTest()
        {
            configuration.Register<I, RecursiveImpl>();

            DI = new DI(configuration);
            var actual = DI.Resolve<I>();
        }

        [TestMethod]
        public void SingletonResolveTest()
        {
            configuration.Register<I, Impl1>(true);

            DI = new DI(configuration);

            var first = DI.Resolve<I>();
            var second = DI.Resolve<I>();
            Assert.AreSame(first, second);
        }

        [TestMethod]
        public void InstanceForEachResolveTest()
        {
            configuration.Register<I, Impl1>();

            DI = new DI(configuration);

            var first = DI.Resolve<I>();
            var second = DI.Resolve<I>();
            Assert.AreNotSame(first, second);
        }

        [TestMethod]
        public void ExplicitNameTest()
        {
            configuration.Register<I, Impl1>(name: "1");
            configuration.Register<I, Impl2>(name: "2");

            DI = new DI(configuration);

            var actual = DI.Resolve<I>("1");
            Assert.AreEqual(typeof(Impl1), actual.GetType());

            actual = DI.Resolve<I>("2");
            Assert.AreEqual(typeof(Impl2), actual.GetType());
        }
    }
}