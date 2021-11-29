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
            Assert(repository.GetType().Equals(typeof(MyRepository)), "types should be equal!");
            Assert(service.GetType().Equals(typeof(ServiceImpl1<IRepository>)), "bla");
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

            Assert(serviceImpl1 == serviceImpl11, "dfg");
            Assert(serviceImpl2 != serviceImpl22, "dfg");
        }

        static void BaseInterfaceTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register(typeof(IRepository), typeof(MyRepository));
            configuration.Register<IBaseService, ServiceImpl1<IRepository>>();
            var container = new DependencyProvider(configuration);
            var service = container.Resolve<IBaseService>();

            Assert(service.GetType().Equals(typeof(ServiceImpl1<IRepository>)), "types should be equal!");
        }

        static void ListOfDependenciesWithOpenGenericsTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register(typeof(IRepository), typeof(MyRepository));
            configuration.Register(typeof(IRepository), typeof(SomeRepository));
            var container = new DependencyProvider(configuration);
            var services = container.Resolve<IEnumerable<IRepository>>().ToList();

            AssertEqual(services.Count, 2, "dfg");

            Assert(services[0].GetType().Equals(typeof(MyRepository)), "types should be equal!");
            Assert(services[1].GetType().Equals(typeof(SomeRepository)), "types should be equal!");
        }

        static void ListOfDependenciesTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register<IRepository, MyRepository>();
            configuration.Register<IRepository, SomeRepository>();
            var container = new DependencyProvider(configuration);
            var services = container.Resolve<IEnumerable<IRepository>>().ToList();

            AssertEqual(services.Count, 2, "asdf");

            Assert(services[0].GetType().Equals(typeof(MyRepository)), "types should be equal!");
            Assert(services[1].GetType().Equals(typeof(SomeRepository)), "types should be equal!");
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
                Console.WriteLine("Caught!!!");
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
                Console.WriteLine("Caught!!! 1");
            }

            try
            {
                container.Resolve<Class2>();
            }
            catch (DependencyException e)
            {
                Console.WriteLine("Caught!!! 2");
            }
        }

        static void WrongIdTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register<IRepository, MyRepository>("kek");
            var container = new DependencyProvider(configuration);

            try
            {
                container.Resolve<IRepository>("wrong");
            }
            catch (DependencyException e)
            {
                Console.WriteLine("Caught!!! 1");
            }

        }

        static void ProvideByIdTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register<IRepository, MyRepository>("my");
            var container = new DependencyProvider(configuration);
            var repository = container.Resolve<IRepository>("my");

            Assert(repository.GetType().Equals(typeof(MyRepository)), "types should be equal!");
        }

        static void ProvidedAttributeTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register<IRepository, MyRepository>();
            configuration.Register<IRepository, MyAnotherRepository>("yes");
            configuration.Register<IService<IRepository>, ServiceImpl3<IRepository>>();
            var container = new DependencyProvider(configuration);
            var service = container.Resolve<IService<IRepository>>();

            Assert(service.GetType().Equals(typeof(ServiceImpl3<IRepository>)), "types should be equal!");
            Assert( ((ServiceImpl3<IRepository>)service).Repository.GetType().Equals(typeof(MyAnotherRepository)), "asdfg");
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
