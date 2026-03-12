# LogicBuilder.Domain

The **LogicBuilder.Domain** project provides foundational classes for building domain models in a layered architecture pattern.

## Overview

This project contains the `BaseModel` abstract class, which serves as the base class for all domain model entities in the application layer. Domain models represent business entities and logic, distinct from data persistence concerns.

## Key Components

### BaseModel

An abstract base class implements the `IBaseData` interface which all domain model entities should implement.

**Features:**
- **EntityState Property**: Tracks the state of the entity (`Unchanged`, `Added`, `Modified`, `Deleted`) using the `EntityStateType` enum from `LogicBuilder.Data`
- **Change Tracking**: Enables generic CRUD operations in `LogicBuilder.EntityFrameworkCore.SqlServer` without coupling domain models directly to Entity Framework

## Usage in LogicBuilder.EntityFrameworkCore.SqlServer

The `BaseModel` class is central to the repository pattern implementation:

1. **ModelRepositoryBase**: Generic repository operations require models to implement `IBaseModel` (constraint: `where TModel : IBaseModel`)
2. **State Management**: The `EntityState` property is mapped from domain models to Entity Framework's `EntityState` during save operations
3. **Graph Operations**: Enables tracking of complex object graphs for insert, update, and delete operations through the `SaveGraphAsync` methods

## Design Pattern

This follows a **layered architecture** approach:
- **LogicBuilder.Domain** (Domain Layer): Business entities implementing `IBaseModel`
- **LogicBuilder.Data** (Data Layer): Database entities implementing `IBaseData` 
- **AutoMapper**: Translates between domain models and data entities
- **LogicBuilder.EntityFrameworkCore.SqlServer**: Provides generic repository pattern supporting both layers

By inheriting from `BaseModel`, your domain entities gain state tracking capabilities while remaining independent of Entity Framework infrastructure concerns.