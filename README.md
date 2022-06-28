# juniper-connector.
## Summary
The project contains an API implementation and static data server for Juniper API

[Juniper Innovating Travel Technology] (https://www.ejuniper.com/)

[API documentation] (https://api-edocs.ejuniper.com/)

### Infrastructure Dependencies
* Database connection string
* Redis instance
* Sentry endpoint
* Access to Vault
* Jaeger instance _(optional)_

### Project Dependencies
The project relies on [Edo.Contracts](https://github.com/happy-travel/edo-contracts) library, that _must_ be in sync with another connectors. Also you might need NetTopologySuite and enabled GIS services on the DB side.

## Static data updater
Data updater is implemented to have two separate steps:
1. Raw data update - loads raw data from the Static data API to the local table
2. Accommodation update - gets data from raw data tables, converts and saves to Accommodations table for further usage in connector

There are three settings, that should be set as environment variables:
- `CS_ACCOMMODATION_UPDATE_MODE` - `Full` or `Incremental`, sets update mode for the accommodation update step
- `CS_WORKERS_TO_RUN` - semicolon splitted list of workers, that should be run.

### Available workers:
- ZoneLoader - to load full zone list from the static API
- HotelLoader - to load full or incremental changes from the static API
- AccommodationUpdater - to load full or incremental changes from the raw data tables

### Typical scenarios
#### Full update
```
CS_ACCOMMODATION_UPDATE_MODE = Full
CS_WORKERS_TO_RUN = ZoneLoader;HotelLoader;AccommodationUpdater
```

#### Incremental update (weekly schedule)
```
CS_ACCOMMODATION_UPDATE_MODE = Incremental
CS_WORKERS_TO_RUN = ZoneLoader;HotelLoader;AccommodationUpdater
```

#### Incremental update (daily schedule)
```
CS_ACCOMMODATION_UPDATE_MODE = Incremental
CS_WORKERS_TO_RUN = HotelLoader;AccommodationUpdater
```

#### Accommodation name normalization
```
CS_ACCOMMODATION_UPDATE_MODE = Full
CS_WORKERS_TO_RUN = AccommodationUpdater
```