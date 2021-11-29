using DependencyInjectionContainer;
using TestsProject;
using TestDemo.ClassesUnderTest;

using static TestsProject.TestFramework;
using System.Collections.Generic;
using System.Linq;
using DependencyInjectionContainer.CustomExceptions;
using System;

namespace TestDemo
{
    class TestDemo
    {
        delegate void test_method();

        static void SimpleGenericTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register<IRepository, MyRepository>();
            configuration.Register<IService<IRepository>, ServiceImpl1<IRepository>>();

            var container = new DependencyProvider(configuration);
            var repository = container.Resolve<IRepository>();
            var service = container.Resolve<IService<IRepository>>();

            Assert(repository.GetType().Equals(typeof(MyRepository)), "MyRepository type was expected!");
            Assert(service.GetType().Equals(typeof(ServiceImpl1<IRepository>)), "ServiceImpl1<IRepository> type was expected!");
        }

        static void SingletonTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register(typeof(IRepository), typeof(MyRepository));
            configuration.Register(typeof(IService<>), typeof(ServiceImpl1<>), LifeCycle.Singleton, "1");
            configuration.Register(typeof(IService<>), typeof(ServiceImpl1<>), LifeCycle.InstancePerDependency, "2");

            var container = new DependencyProvider(configuration);
            var serviceImpl1 = container.Resolve<IService<IRepository>>("1");
            var serviceImpl2 = container.Resolve<IService<IRepository>>("2");

            var serviceImpl11 = container.Resolve<IService<IRepository>>("1");
            var serviceImpl22 = container.Resolve<IService<IRepository>>("2");

            Assert(serviceImpl1 == serviceImpl11, "The very same object was expected! (Singleton)");
            Assert(serviceImpl2 != serviceImpl22, "A newly created object was expected! (InstancePerDependency)");
        }

        static void BaseInterfaceTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register(typeof(IRepository), typeof(MyRepository));
            configuration.Register<IBaseService, ServiceImpl1<IRepository>>();

            var container = new DependencyProvider(configuration);
            var service = container.Resolve<IBaseService>();

            Assert(service.GetType().Equals(typeof(ServiceImpl1<IRepository>)), "ServiceImpl1<IRepository> was expected!");
        }

        static void ListOfDependenciesWithOpenGenericsTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register(typeof(IRepository), typeof(MyRepository));
            configuration.Register(typeof(IRepository), typeof(SomeRepository));

            var container = new DependencyProvider(configuration);
            var services = container.Resolve<IEnumerable<IRepository>>().ToList();

            AssertEqual(services.Count, 2, "IRepository is supposed to be wired up against 2 implementations! (open generics)");
            Assert(services[0].GetType().Equals(typeof(MyRepository)), "First implementation has to be MyRepository!");
            Assert(services[1].GetType().Equals(typeof(SomeRepository)), "Second implementation has to be SomeRepository!");
        }

        static void ListOfDependenciesTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register<IRepository, MyRepository>();
            configuration.Register<IRepository, SomeRepository>();

            var container = new DependencyProvider(configuration);
            var services = container.Resolve<IEnumerable<IRepository>>().ToList();

            AssertEqual(services.Count, 2, "asdf");
            Assert(services[0].GetType().Equals(typeof(MyRepository)), "IRepository is supposed to be wired up against 2 implementations! (open generics)");
            Assert(services[1].GetType().Equals(typeof(SomeRepository)), "Second implementation has to be SomeRepository");
        }

        static void RecursionTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register<SelfDependent, SelfDependent>();
            var container = new DependencyProvider(configuration);

            try
            {
                container.Resolve<SelfDependent>();
            }
            catch(DependencyException e)
            {
                Console.WriteLine("Class mapping to itself has been evaded!");
            }
        }

        static void CyclicDependencyTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register<Class1, Class1>();
            configuration.Register<Class2, Class2>();
            var container = new DependencyProvider(configuration);

            try
            {
                container.Resolve<Class1>();
            }
            catch (DependencyException e)
            {
                Console.WriteLine("Cyclic dependency has been evaded! (Class1)");
            }

            try
            {
                container.Resolve<Class2>();
            }
            catch (DependencyException e)
            {
                Console.WriteLine("Cyclic dependency has been evaded! (Class2)");
            }
        }

        static void WrongIdTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register<IRepository, MyRepository>("SuchAUniqueAndTotallyUnexpectedNameWhoWhouldveThought");
            var container = new DependencyProvider(configuration);

            try
            {
                container.Resolve<IRepository>("wrongName");
            }
            catch (DependencyException e)
            {
                Console.WriteLine("Unexpected name has been intercepted!");
            }

        }

        static void ProvideByIdTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register<IRepository, MyRepository>("my");

            var container = new DependencyProvider(configuration);
            var repository = container.Resolve<IRepository>("my");

            Assert(repository.GetType().Equals(typeof(MyRepository)), "Resolution by name has not been successful!");
        }

        static void ProvidedAttributeTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register<IRepository, MyRepository>();
            configuration.Register<IRepository, MyAnotherRepository>("yes");
            configuration.Register<IService<IRepository>, ServiceImpl3<IRepository>>();

            var container = new DependencyProvider(configuration);
            var service = container.Resolve<IService<IRepository>>();

            Assert(service.GetType().Equals(typeof(ServiceImpl3<IRepository>)), "ServiceImpl3<IRepository>");
            Assert( ((ServiceImpl3<IRepository>)service).Repository.GetType().Equals(typeof(MyAnotherRepository)), "Named dependency failed!");
        }

        static void Main(string[] args)
        {
            test_method testDelegate;
            TestRunner<test_method> r = new TestRunner<test_method>();

            testDelegate = SimpleGenericTest;
            r.RunTest(testDelegate, "SimpleGenericTest");

            testDelegate = SingletonTest;
            r.RunTest(testDelegate, "SingletonTest");

            testDelegate = BaseInterfaceTest;
            r.RunTest(testDelegate, "BaseInterfaceTest");

            testDelegate = ListOfDependenciesWithOpenGenericsTest;
            r.RunTest(testDelegate, "ListOfDependenciesWithOpenGenericsTest");

            testDelegate = ListOfDependenciesTest;
            r.RunTest(testDelegate, "ListOfDependenciesTest");

            testDelegate = RecursionTest;
            r.RunTest(testDelegate, "RecursionTest");

            testDelegate = CyclicDependencyTest;
            r.RunTest(testDelegate, "CyclicDependencyTest");

            testDelegate = WrongIdTest;
            r.RunTest(testDelegate, "WrongIdTest");

            testDelegate = ProvideByIdTest;
            r.RunTest(testDelegate, "ProvideByIdTest");

            testDelegate = ProvidedAttributeTest;
            r.RunTest(testDelegate, "ProvidedAttributeTest");
        }
    }
}
