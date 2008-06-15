using System;
using NUnit.Framework;
using StructureMap.Configuration.DSL;
using StructureMap.Testing.Widget3;

namespace StructureMap.Testing.Configuration.DSL
{
    [TestFixture]
    public class InterceptAllInstancesOfPluginTypeTester : Registry
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            _lastService = null;
            _manager = null;

            _defaultRegistry = (registry =>
            {
                registry.ForRequestedType<IService>()
                    .AddInstances(
                    Instance<ColorService>().WithName("Red").WithProperty("color").
                        EqualTo(
                        "Red"),
                    Object<IService>(new ColorService("Yellow")).WithName("Yellow"),
                    ConstructedBy<IService>(
                        delegate { return new ColorService("Purple"); })
                        .WithName("Purple"),
                    Instance<ColorService>().WithName("Decorated").WithProperty("color")
                        .
                        EqualTo("Orange")
                    );
            });
        }

        #endregion

        private IService _lastService;
        private IContainer _manager;
        private Action<Registry> _defaultRegistry;

        private IService getService(Action<Registry> action, string name)
        {
            if (_manager == null)
            {
                _manager = new Container(registry =>
                {
                    _defaultRegistry(registry);
                    action(registry);
                });
            }

            return _manager.GetInstance<IService>(name);
        }

        [Test]
        public void EnrichForAll()
        {
            Action<Registry> action = registry => registry.ForRequestedType<IService>()
                                                      .EnrichWith(s => new DecoratorService(s))
                                                      .AddInstance(
                                                      ConstructedBy<IService>(() => new ColorService("Green"))
                                                          .WithName("Green"));


            IService green = getService(action, "Green");


            var decoratorService = (DecoratorService) green;
            var innerService = (ColorService) decoratorService.Inner;
            Assert.AreEqual("Green", innerService.Color);
        }

        [Test]
        public void OnStartupForAll()
        {
            Action<Registry> action = registry => registry.ForRequestedType<IService>()
                                                      .OnCreation(s => _lastService = s)
                                                      .AddInstance(
                                                      ConstructedBy<IService>(() => new ColorService("Green"))
                                                          .WithName("Green"));


            IService red = getService(action, "Red");
            Assert.AreSame(red, _lastService);

            IService purple = getService(action, "Purple");
            Assert.AreSame(purple, _lastService);

            IService green = getService(action, "Green");
            Assert.AreSame(green, _lastService);

            IService yellow = getService(action, "Yellow");
            Assert.AreEqual(yellow, _lastService);
        }
    }
}