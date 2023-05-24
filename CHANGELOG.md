# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/pt-BR/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html)

## [0.2.0] - 2023-05-24
### Added
- Log support w/ Serilog
- Include Unit Tests project w/ xUnit

## [0.1.0] - 2023-05-23
### Added
- Project core v0.1.0
- Order Book from Bitstamp (Websocket version)
- Bitcoin and Etherium support
- Database storage w/ MongoDb to keep Order Book history
- Live route (SSE) to get Order Book ask/bid information
- Endpoint for best price calculation
- Docker and docker-compose support
- Console application to allow Bitstamp Order Book testing
