# The paket.dependencies file

The `paket.dependencies` file is used to specify rules regarding your application's dependencies.

To give you an overview, consider the following `paket.dependencies` file:

    source https://nuget.org/api/v2

    // NuGet packages
    nuget NUnit ~> 2.6.3
    nuget FAKE ~> 3.4
    nuget DotNetZip >= 1.9
    nuget SourceLink.Fake

    // Files from GitHub repositories
    github forki/FsUnit FsUnit.fs

    // Gist files
    gist Thorium/1972349 timestamp.fs

    // HTTP resources
    http http://www.fssnip.net/1n decrypt.fs

The file specifies that Paket's NuGet dependencies should be downloaded from [nuget.org](http://www.nuget.org) and that we need:

  * [NUnit](http://www.nunit.org/) in version [2.6.3 <= x < 2.7](nuget-dependencies.html#Pessimistic-version-constraint)
  * [FAKE](http://fsharp.github.io/FAKE/) in version [3.4 <= x < 4.0](nuget-dependencies.html#Pessimistic-version-constraint) as a build tool
  * [DotNetZip](http://dotnetzip.codeplex.com/) with version which is at [least 1.9](http://fsprojects.github.io/Paket/nuget-dependencies.html#Greater-than-or-equal-version-constraint)
  * [SourceLink.Fake](https://github.com/ctaggart/SourceLink) in the latest version
  * [FSUnit.fs](https://github.com/forki/FsUnit) from GitHub.
  * Gist number [1972349](https://gist.github.com/Thorium/1972349) from GitHub Gist
  * External HTTP resource, e.g. [1n](http://www.fssnip.net/1n) from [FSSnip](http://www.fssnip.net/)

Paket uses this definition to compute a concrete dependency resolution, which also includes transitive dependencies. The resulting dependency graph is then persisted to the [`paket.lock` file](lock-file.html).

Only direct dependencies should be listed and you can use the [`paket simplify` command](paket-simplify.html) to remove transitive dependencies.

## Sources

Paket supports the following source types:

* [NuGet](nuget-dependencies.html)
* [GitHub and Gist](github-dependencies.html)
* [HTTP](http-dependencies.html) (any single file from any site without version control)

## Global options

### Strict references

Paket usually references all direct and transitive dependencies that are listed in your [`paket.references` files](references-files.md) to your project file.
In `strict` mode it will **only** reference *direct* dependencies.

    references: strict
    source https://nuget.org/api/v2

    nuget Newtonsoft.Json ~> 6.0
    nuget UnionArgParser ~> 0.7

### Framework restrictions

Sometimes you don't want to generate dependencies for older framework versions. You can control this in the [`paket.dependencies` file](dependencies-file.html):

    framework: net35, net40
    source https://nuget.org/api/v2

    nuget Example >= 2.0 // only .NET 3.5 and .NET 4.0

### No content option

This option disables the installation of any content files:

    content: none
    source https://nuget.org/api/v2

    nuget jQuery >= 0 // we don't install jQuery content files
    nuget UnionArgParser ~> 0.7

### import_targets settings

If you don't want to import `.targets` and `.props` files you can disable it via the `import_targets` switch:

    import_targets: false
    source https://nuget.org/api/v2

    nuget Microsoft.Bcl.Build // we don't import .targets and .props
    nuget UnionArgParser ~> 0.7

### copy_local settings

It's possible to influence the `Private` property for references via the `copy_local` switch:

    copy_local: false
    source https://nuget.org/api/v2

    nuget Newtonsoft.Json

### Redirects option

This option tells paket to create [Assembly Binding Redirects](https://msdn.microsoft.com/en-us/library/433ysdt1(v=vs.110).aspx) for all referenced libraries.

    redirects: on
    source https://nuget.org/api/v2

    nuget UnionArgParser ~> 0.7

### Strategy option

This option tells Paket what resolver strategy it will use by default. 

NuGet's dependency syntax led to a lot of incompatible packages on Nuget.org. To make your transition to Paket easier and to allow package authors to correct their version constraints you can have Paket behave like NuGet when resolving transitive dependencies (i.e. defaulting to lowest matching versions).

The strategy can be either `min` or `max`.

    strategy: min
    source https://nuget.org/api/v2

    nuget UnionArgParser ~> 0.7

A `min` strategy means you get the *lowest matching version* of your transitive dependencies (i.e. NuGet-style). In constrast, a `max` strategy will get you the *highest matching version*.

Note, however, that all direct dependencies still get their *lastest matching versions*, no matter the value of the `strategy` option. 

The only exception is when you are updating a single package and one of your direct dependencies is a transitive dependency for that specific package. In this case, only the updating package will get its *latest matching version* and the dependency is treated as transitive.

To override a strategy, you can use the one of the [strategy modifiers](nuget-dependencies.html#Strategy-modifiers).

## Comments

All lines starting with with `//` or `#` are considered comments.
