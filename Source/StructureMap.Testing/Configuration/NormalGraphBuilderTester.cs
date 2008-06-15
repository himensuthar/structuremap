using System;
using NUnit.Framework;
using StructureMap.Configuration;
using StructureMap.Configuration.DSL;
using StructureMap.Graph;
using StructureMap.Pipeline;
using StructureMap.Testing.Widget3;

namespace StructureMap.Testing.Configuration
{
    [TestFixture]
    public class NormalGraphBuilderTester
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
        }

        #endregion

        [Test]
        public void AddAssembly_HappyPath()
        {
            GraphBuilder builder = new GraphBuilder(new Registry[0]);
            string assemblyName = GetType().Assembly.GetName().Name;
            builder.AddAssembly(assemblyName);

            Assert.IsTrue(builder.PluginGraph.Assemblies.Contains(assemblyName));
            Assert.AreEqual(0, builder.PluginGraph.Log.ErrorCount);
        }

        [Test]
        public void AddAssembly_SadPath()
        {
            GraphBuilder builder = new GraphBuilder(new Registry[0]);
            builder.AddAssembly("something");

            builder.PluginGraph.Log.AssertHasError(101);
        }

        [Test]
        public void Call_the_action_on_configure_family_if_the_pluginType_is_found()
        {
            TypePath typePath = new TypePath(typeof (IGateway));

            bool iWasCalled = false;
            GraphBuilder builder = new GraphBuilder(new Registry[0]);
            builder.ConfigureFamily(typePath, f =>
            {
                Assert.AreEqual(typeof (IGateway), f.PluginType);
                iWasCalled = true;
            });


            Assert.IsTrue(iWasCalled);
        }

        [Test]
        public void Configure_a_family_that_does_not_exist_and_log_an_error_with_PluginGraph()
        {
            GraphBuilder builder = new GraphBuilder(new Registry[0]);
            builder.ConfigureFamily(new TypePath("a,a"), delegate { });

            builder.PluginGraph.Log.AssertHasError(103);
        }

        [Test]
        public void Create_system_object_successfully_and_call_the_requested_action()
        {
            MemoryInstanceMemento memento = new MemoryInstanceMemento("Singleton", "anything");

            bool iWasCalled = false;

            GraphBuilder builder = new GraphBuilder(new Registry[0]);
            builder.PrepareSystemObjects();
            builder.WithSystemObject<IBuildInterceptor>(memento, "singleton", policy =>
            {
                Assert.IsInstanceOfType(typeof (SingletonPolicy), policy);
                iWasCalled = true;
            });

            Assert.IsTrue(iWasCalled);
        }

        [Test]
        public void Do_not_call_the_action_on_ConfigureFamily_if_the_type_path_blows_up()
        {
            GraphBuilder builder = new GraphBuilder(new Registry[0]);
            builder.ConfigureFamily(new TypePath("a,a"), obj => Assert.Fail("Should not be called"));
        }

        [Test]
        public void Do_not_try_to_execute_the_action_when_requested_system_object_if_it_cannot_be_created()
        {
            MemoryInstanceMemento memento = new MemoryInstanceMemento();
            GraphBuilder builder = new GraphBuilder(new Registry[0]);

            builder.WithSystemObject<MementoSource>(memento, "I am going to break here",
                                                    delegate { Assert.Fail("Wasn't supposed to be called"); });
        }

        [Test]
        public void Log_an_error_for_a_requested_system_object_if_it_cannot_be_created()
        {
            MemoryInstanceMemento memento = new MemoryInstanceMemento();
            GraphBuilder builder = new GraphBuilder(new Registry[0]);

            builder.WithSystemObject<MementoSource>(memento, "I am going to break here", delegate { });

            builder.PluginGraph.Log.AssertHasError(130);
        }

        [Test]
        public void WithType_calls_through_to_the_Action_if_the_type_can_be_found()
        {
            GraphBuilder builder = new GraphBuilder(new Registry[0]);
            bool iWasCalled = true;

            builder.WithType(new TypePath(GetType()), "creating a Plugin", t =>
            {
                iWasCalled = true;
                Assert.AreEqual(GetType(), t);
            });

            Assert.IsTrue(iWasCalled);
        }

        [Test]
        public void WithType_fails_and_logs_error_with_the_context()
        {
            GraphBuilder builder = new GraphBuilder(new Registry[0]);
            builder.WithType(new TypePath("a,a"), "creating a Plugin", obj => Assert.Fail("Should not be called"));

            builder.PluginGraph.Log.AssertHasError(131);
        }
    }
}