using DependencyInjectionContainer.CustomAttributes;

namespace TestDemo.ClassesUnderTest
{
    public class SelfDependent
    {
        public SelfDependent(SelfDependent self) {}
    }
}