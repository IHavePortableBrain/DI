namespace DependencyInjector.Test.Class
{
    internal class NamedDependencyConstructorImpl
    {
        public I I1;
        public I I2;

        public NamedDependencyConstructorImpl([DependencyKey(name: "1")] I impl1, [DependencyKey(name: "2")] I impl2)
        {
            I1 = impl1;
            I2 = impl2;
        }
    }
}