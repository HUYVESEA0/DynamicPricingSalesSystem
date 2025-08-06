# Dynamic Pricing Sales System

![.NET](https://img.shields.io/badge/.NET-8.0-blue) ![C#](https://img.shields.io/badge/C%23-12.0-green) ![License](https://img.shields.io/badge/License-MIT-yellow)

A comprehensive sales management system with an intelligent **Dynamic Pricing Engine** that automatically adjusts product prices in real-time based on multiple factors including supply-demand, competitor analysis, seasonality, inventory levels, and customer behavior.

## 🌟 Features

### 🤖 Dynamic Pricing Engine
- **Smart Pricing Algorithm**: Automatically calculate optimal prices based on:
  - Competitor pricing analysis
  - Inventory levels and stock ratios
  - Demand patterns and sales trends
  - Seasonal factors and time-based pricing
  - Customer segmentation and behavior
  - Target profit margins
- **Pricing Rules Engine**: Automated rules system with customizable conditions
- **Real-time Price Updates**: Live price adjustments with preview and apply modes
- **Price History & Analytics**: Track pricing strategy effectiveness

### 📊 Sales Management Core
- **Product Management**: Full CRUD operations with category management
- **Customer Management**: Customer database with intelligent segmentation
- **Order Processing**: Order handling with dynamic pricing integration
- **Inventory Tracking**: Real-time stock management with low-stock alerts
- **Sales Reports**: Comprehensive sales analytics and insights

### 📈 Advanced Analytics & Reporting
- **Revenue Optimization Dashboard**: Key performance indicators and metrics
- **Price Elasticity Analysis**: Price sensitivity analysis for products
- **Profit Margin Tracking**: Profitability monitoring across categories
- **Customer Behavior Analytics**: Patterns, insights, and churn prediction
- **Competitive Intelligence**: Market positioning and competitor analysis

### 🧠 AI-Powered Features
- **Demand Forecasting**: Predict future demand based on historical data
- **Price Optimization**: Machine learning for optimal pricing strategies
- **Customer Lifetime Value**: CLV calculations and predictions
- **Customer Segmentation**: Automatic VIP, Regular, New, At-risk categorization

## 🏗️ Architecture

### Project Structure
```
DynamicPricingSalesSystem/
├── Models/                     # Core data models
│   ├── Product.cs             # Product model with pricing constraints
│   ├── Customer.cs            # Customer model with segmentation
│   ├── Order.cs & OrderItem.cs # Order management models
│   ├── PricingRule.cs         # Pricing rules configuration
│   ├── PriceHistory.cs        # Price change tracking
│   ├── Competitor.cs          # Competitor data models
│   └── SalesMetrics.cs        # Analytics and metrics models
├── Services/                   # Business logic layer
│   ├── ProductService.cs      # Product management operations
│   ├── CustomerService.cs     # Customer management operations
│   ├── OrderService.cs        # Order processing logic
│   ├── AnalyticsService.cs    # Analytics and reporting
│   └── [Additional Services]
├── Engines/                    # Core pricing engines
│   ├── DynamicPricingEngine.cs # Main pricing logic
│   ├── DemandForecastEngine.cs # Demand prediction
│   └── MLPricingEngine.cs     # Machine learning pricing
├── Data/                       # Data management layer
│   ├── JsonDataManager.cs     # JSON persistence manager
│   └── SampleDataGenerator.cs # Sample data generation
├── Utils/                      # Utility classes
│   ├── ConsoleHelper.cs       # Console UI utilities
│   ├── ChartGenerator.cs      # Data visualization
│   └── PricingCalculator.cs   # Pricing algorithms
├── Program.cs                  # Application entry point
└── MainMenu.cs                # Console interface
```

## 🚀 Getting Started

### Prerequisites
- .NET 8.0 SDK or later
- Visual Studio 2022 / VS Code / JetBrains Rider (optional)

### Installation
1. Clone the repository:
   ```bash
   git clone https://github.com/HUYVESEA0/DynamicPricingSalesSystem.git
   cd DynamicPricingSalesSystem
   ```

2. Build the project:
   ```bash
   dotnet build
   ```

3. Run the application:
   ```bash
   dotnet run
   ```

### First Run
The system will automatically generate sample data on first launch, including:
- 20 diverse products across multiple categories
- 30 customers with different segments and behaviors
- 50 historical orders for algorithm testing
- Competitive pricing data
- Pricing rules and configurations

## 💻 Usage

### Main Menu Navigation
```
============================================================
                 DYNAMIC PRICING SALES SYSTEM
============================================================

1. Product Management       - Manage inventory and product catalog
2. Customer Management     - Handle customer data and segmentation
3. Order Management        - Process orders and track fulfillment
4. Dynamic Pricing         - Configure and run pricing algorithms
5. Analytics & Reports     - View insights and performance metrics
6. Data Management         - Backup, export, and manage data
7. Dashboard              - Real-time business overview
Q. Quit
```

### Dynamic Pricing Operations

#### 1. Price Analysis
View optimal pricing recommendations for all products:
```
Product ID  Current  Optimal  Difference  Change %  
----------------------------------------------------
10          $188.95  $125.56  $-63.39     -33.5%    
2           $223.00  $160.84  $-62.16     -27.9%    
17          $190.35  $131.10  $-59.25     -31.1%    
```

#### 2. Pricing Recommendations
Get detailed recommendations for individual products including:
- Confidence levels
- Reasoning behind price changes
- Expected impact on sales and revenue

#### 3. Automated Price Updates
- **Preview Mode**: See what changes would be made
- **Apply Mode**: Execute price changes across the catalog
- **Manual Override**: Set prices manually with reason tracking

### Sample Dashboard Output
```
Key Metrics (Last 30 Days)
--------------------------
Total Revenue: $8,184.45
Total Orders: 15
Active Customers: 26
Average Order Value: $744.04
Low Stock Products: 2
Total Products: 20

Category Performance
--------------------
Toys: 4 products
Clothing: 4 products
Health: 2 products
Sports: 3 products
```

## 🧮 Pricing Algorithms

### Supply-Demand Algorithm
- Analyzes recent sales velocity
- Considers stock ratios and inventory pressure
- Adjusts prices based on market demand signals

### Competitor-Based Pricing
- Benchmarks against competitor prices
- Maintains competitive positioning
- Applies brand premium calculations

### Time-Based Pricing
- Weekend and holiday premiums
- Peak hour adjustments
- Seasonal pricing multipliers

### Customer Segment Pricing
- VIP customer discounts
- Price-sensitive customer adjustments
- Loyalty-based pricing strategies

## 📊 Analytics Features

### Key Performance Indicators
- Revenue trends and growth metrics
- Customer acquisition and retention rates
- Inventory turnover and optimization
- Pricing strategy effectiveness

### Customer Analytics
- Customer Lifetime Value (CLV) calculations
- Churn prediction and risk assessment
- Purchase behavior patterns
- Segment performance analysis

### Product Analytics
- Price elasticity measurements
- Demand forecasting accuracy
- Profit margin optimization
- Category performance comparisons

## 🔧 Configuration

### Pricing Rules
Configure automated pricing rules through the system:
```csharp
new PricingRule
{
    Name = "Low Stock Premium",
    RuleType = PricingRuleType.InventoryBased,
    Conditions = "Stock < 10",
    MinPriceMultiplier = 1.0m,
    MaxPriceMultiplier = 1.3m,
    Description = "Increase price when stock is low"
}
```

### Customer Segments
Automatic segmentation based on:
- **VIP**: High spending, high loyalty
- **Regular**: Consistent purchasing behavior
- **New**: Recently registered customers
- **At-Risk**: Declining engagement patterns

## 💾 Data Management

### JSON Persistence
- All data stored in human-readable JSON format
- Automatic backup functionality
- Easy data export and import
- Data integrity validation

### Sample Data
Generate realistic test data including:
- Product catalogs with pricing constraints
- Customer profiles with purchase history
- Order history with seasonal patterns
- Competitor pricing data

## 🔍 Technical Highlights

### Clean Architecture
- **SOLID principles** implementation
- **Separation of concerns** throughout
- **Dependency injection** ready
- **Unit test friendly** structure

### Performance Optimization
- Efficient pricing calculation algorithms
- In-memory caching for frequently accessed data
- Optimized data structures for analytics
- Minimal memory footprint

### Extensibility
- Plugin-ready pricing engine architecture
- Configurable rules engine
- Modular service design
- Easy integration points for external systems

## 🎯 Business Value

### For Small to Medium Enterprises
- **Competitive Advantage**: AI-powered pricing optimization
- **Revenue Optimization**: Automatic margin improvement
- **Operational Efficiency**: Reduced manual pricing tasks
- **Market Intelligence**: Competitor analysis and positioning

### For Developers
- **Learning Opportunity**: Advanced C# patterns and algorithms
- **Portfolio Project**: Showcase of business logic implementation
- **Algorithm Implementation**: Pricing and forecasting models
- **System Design**: Complete business application architecture

## 🚀 Future Enhancements

### Planned Features
- [ ] Machine Learning integration with ML.NET
- [ ] Web API for external system integration
- [ ] Real-time competitor price scraping
- [ ] Advanced forecasting models
- [ ] Multi-currency support
- [ ] A/B testing framework for pricing strategies

### Integration Possibilities
- E-commerce platform connectors
- ERP system integration
- Business intelligence tools
- Cloud deployment options

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

## ⭐ Acknowledgments

- Inspired by modern e-commerce pricing strategies
- Built with .NET 8.0 and modern C# practices
- Designed for educational and commercial use

---

**Dynamic Pricing Sales System** - Revolutionizing pricing strategies with intelligent automation 🚀
