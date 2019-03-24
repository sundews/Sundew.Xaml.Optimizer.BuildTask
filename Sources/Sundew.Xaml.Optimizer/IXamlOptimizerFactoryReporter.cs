namespace Sundew.Xaml.Optimizer.Factory
{
    /// <summary>Interface for implementing a reporter for <see cref="XamlOptimizerFactory"/>.</summary>
    public interface IXamlOptimizerFactoryReporter
    {
        /// <summary>Founds the settings.</summary>
        /// <param name="path">The path.</param>
        void FoundSettings(string path);
    }
}