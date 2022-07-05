# Quick Start
Getting started with the Carbon Aware SDK

## dotnet
The dotnet SDK is built in .NET 6.0.  It supports devcontainers, and can be buitl via command line or via Visual Studio.  This quickstart assumes you know how to use dev containers.  To learn more about dev containers please check out _link here_

1. First step is to clone the project with git
<pre>$ git clone command here</pre>
2. Open the root folder of the project in Visual Studio Code
3. Bring up the console with ctrl-` 
4. Change to the src/build directory by typing the following in the console
<pre>$ cd src/build</pre>
5. Publish the build
<pre>$ dotnet publish .. -o .</pre>
6. Run with the hello world test data set
<pre>$ CarbonAwareCLI -l westus eastus -d "data-files/hello-world.json"</pre>
7. You should see the following results
<pre>TODO: results sample here</pre>
8. Sucess!  You now have the CLI running in the dev container
9. The build folder will also have all class libraries to add to your own dotnet project.  Check out the "Sample Client" project in the solution for an example of where to start

# Next Steps
For more advanced capabilties and to learn more about what you can do with the CLI, please refer to the Carbon Aware CLI documentation.