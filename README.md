# Sundew.Xaml.Optimizer.BuildTask

This project contains a MSBuild task that can be used to optimize XAML files at build time.

## Usage
* Reference the [Sundew.Xaml.Optimizer.BuildTask](https://www.nuget.org/packages/Sundew.Xaml.Optimizer.BuildTask) package
* Reference an optimizer package(s)
* Configure the optimizers by placing "settings.sxos" json file(s) in the "sxos" folder in the project root. Only analyzers enabled by the config will be executed. See [Sample](https://github.com/sundews/Sundew.Xaml.Optimizers.Wpf/tree/master/Source/Sundew.Xaml.Optimizers.Wpf/content/Sundew.Xaml.Optimizers.sxos).
* Build

## Creating custom optimizers
To implement a XamlOptimizer refer to: [Sundew.Xaml.Optimization](https://github.com/sundews/Sundew.Xaml.Optimization)