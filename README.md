##Project Title
API Broker

##Description
This code sample represents a simple implementation of a proxy class that selects the best provider API based on performance metrics and calls the selected API to retrieve location data.

The Proxy class contains a list of ProviderModel, which represent the different providers available to the proxy. Each provider has a name, chance of error, and a RPM limit. The class also contains a dictionary of ProviderStatsModel, which are used to record the statistics of each API call.

The Proxy class implements the RunAsync method which calls GetLocationFromProviders. The GetLocationFromProviders method selects the best provider by calling GetBestProvider, and then mocks the API state, records the API call stats, and returns true if the API call is successful; otherwise, it removes the selected provider from the list and tries again with another provider.

The GetBestProvider method compares the performance metrics of providers and calculates an overall score to select the best provider based on error rate, average response time and whether the RPM limit has crossed or not.
