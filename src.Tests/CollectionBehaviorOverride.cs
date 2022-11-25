using Xunit;

// This is important as the test of calculateclinicaltestresult must
// happen in a specfic order. Therefore parallelization is disabled?
[assembly: CollectionBehavior(DisableTestParallelization = true)]