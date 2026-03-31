# Restaurant Chain Management System

A console-based .NET application for managing a restaurant branch's daily operations, including customers, orders, kitchen workflow, delivery handling, inventory, and feedback.

## Overview

This solution is organized into three projects:

- `RestaurantChainManagementSystem.Core`: domain entities, enums, validation rules, service layer, and contracts
- `RestaurantChainManagementSystem.Infrastructure`: JSON file persistence and seed data
- `RestaurantChainManagementSystem.UI`: console menus and user interaction

The application stores its data in:

- `RestaurantChainManagementSystem.Infrastructure/Data/restaurant-data.json`

If the data file does not exist, or if it becomes unreadable, the app recreates it from the seed builder.

## Requirements

- .NET 10 SDK
- Windows environment matching the current project path layout

## Run

From the repository root:

```bash
dotnet run --project RestaurantChainManagementSystem.UI
```

## Login Flow

The app uses a simple employee-ID based login.

- No password authentication is implemented
- The user selects an existing employee record
- The displayed menu depends on that employee's role

## Seeded Roles

The seeded data includes these employee roles:

- Branch Manager
- Waiter
- Cashier
- Chef
- Delivery Staff

## Main Business Rules

The current implementation enforces these rules:

- Only Waiters and Cashiers can place orders
- The employee handling an order must belong to the same branch as the order
- An order must contain at least one item, and item quantity must be greater than zero
- Branch-specific menu availability is enforced when placing orders
- Branch-specific price overrides are used when calculating order item prices
- Delivery orders require a delivery address and automatically create a delivery record
- Only Chefs can move orders from `Pending` to `Preparing` and from `Preparing` to `Served`
- Inventory is deducted when an order moves to `Preparing`
- If stock is insufficient, preparation is blocked unless a same-branch Branch Manager provides an override
- Only Cashiers can process payments, and only when the order is in `Served`
- Loyalty points are added as `floor(total amount)` on the cashier payment path
- Waiters and Cashiers can cancel pending orders only
- Branch Managers can cancel broader in-progress orders, except completed, already cancelled, or already out-for-delivery orders
- Delivery staff can be assigned only to served delivery orders
- Only Waiters, Cashiers, or Branch Managers can assign delivery staff
- A delivery staff member must be available and from the same branch
- Only the assigned delivery staff member can mark a delivery as `Delivered` or `Failed`
- Marking a delivery as `Delivered` completes the order automatically
- Marking a delivery as `Failed` requires a reason and cancels the order
- Feedback can only be submitted for completed orders
- Only one feedback record is allowed per customer per order

## Project Structure

```text
RestaurantChainManagementSystem/
├── RestaurantChainManagementSystem.Core/
├── RestaurantChainManagementSystem.Infrastructure/
├── RestaurantChainManagementSystem.UI/
├── restaurant.html
└── RestaurantChainManagementSystem.slnx
```

## Notes

- The UI is intentionally console-based; no GUI or web frontend is included
- Data persistence is file-based JSON, not a database
- `restaurant.html` contains the analysis and documentation for the project
