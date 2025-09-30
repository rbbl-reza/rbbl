# RBBL Building Blocks Library

A comprehensive set of foundational components for building domain-driven .NET applications with clean architecture patterns.

## 📦 Overview

This library provides essential building blocks for creating maintainable, testable, and scalable .NET applications. It includes abstractions and implementations for common cross-cutting concerns following domain-driven design principles.

## 🚀 Installation

Add the NuGet package to your project:

```xml
<PackageReference Include="rbbl.buildingblocks" Version="1.0.0" />
```

## 🏗️ Core Components

### 1. Domain-Driven Design Foundation

#### BaseEntity
The foundation for all domain entities with built-in auditing tracking and domain event support.

```csharp
public class Product : BaseEntity
{
    public string Name { get; private set; }
    public decimal Price { get; private set; }

    public Product(string name, decimal price)
    {
        Name = name;
        Price = price;
        
        // Raise domain event
        RaiseDomainEvent(new ProductCreatedDomainEvent(DateTime.UtcNow, Id));
    }
    
    public void UpdatePrice(decimal newPrice)
    {
        Price = newPrice;
        SetModified(); // Update audit fields
    }
}
```

##### Key Features:

- Automatic GUID generation for Id
- Audit fields (CreatedAt, CreatedBy, ModifiedAt, ModifiedBy)
- Built-in domain event collection
- Audit methods: SetCreated(), SetModified()

#### Domain Events

Implement Domain Events to decouple business logic

```csharp
public record ProductCreatedDomainEvent(DateTime OccurredOnUtc, Guid ProductId) : DomainEvent(OccurredOnUtc);

// Event handler
public class ProductCreatedEventHandler
{
    public async Task Handle(ProductCreatedDomainEvent @event)
    {
        // Handle the event (e.g., send notifications, update read models)
    }
}
```

##### Key Features:

- Immutable record types
- Automatic timestamping
- Integration with BaseEntity

#### IHasDomainEvents Interface

```csharp
// Already implemented by BaseEntity
public interface IHasDomainEvents
{
    IReadOnlyCollection<DomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
```
### 2. Repository Pattern

#### Generic Repository Interface

```csharp
public class ProductService
{
    private readonly IRepository<Product> _productRepository;
    
    public ProductService(IRepository<Product> productRepository)
    {
        _productRepository = productRepository;
    }
    
    public async Task<Product?> GetProductAsync(Guid id, CancellationToken ct = default)
    {
        return await _productRepository.GetByIdAsync(id, ct);
    }
    
    public async Task CreateProductAsync(Product product, CancellationToken ct = default)
    {
        await _productRepository.AddAsync(product, ct);
    }
    
    public IQueryable<Product> GetActiveProducts()
    {
        return _productRepository.Query(p => p.IsActive);
    }
}
```

##### Repository Methods:
- GetByIdAsync - Retrieve by primary key
- AddAsync - Add new entity
- UpdateAsync - Update existing entity
- DeleteAsync - Remove entity
- Query - Flexible querying with LINQ support

#### Unit of Work
```csharp
public class OrderService
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<OrderItem> _orderItemRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task CreateOrderAsync(Order order, List<OrderItem> items, CancellationToken ct = default)
    {
        await _orderRepository.AddAsync(order, ct);
        
        foreach (var item in items)
        {
            await _orderItemRepository.AddAsync(item, ct);
        }
        
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
```

### 3. Guard Clauses

Protect your methods with runtime validation:

```csharp
public class User
{
    public User(string email, string name, int age)
    {
        Email = Guard.NotNullOrWhiteSpace(email, nameof(email));
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));
        Age = Guard.InRange(age, 0, 150, nameof(age));
        
        // Multiple validations
        Guard.That(email.Contains("@"), "Invalid email format", nameof(email));
        Guard.MaxLength(name, 100, nameof(name));
    }
    
    public string Email { get; }
    public string Name { get; }
    public int Age { get; }
}
```

#### Available Guard Methods:
- NotNull<T> - Ensures reference types are not null
- NotNullOrWhiteSpace - Ensures strings are valid
- NotEmpty - Ensures GUID is not empty
- NotDefault - Ensures value types are not default
- InRange - Ensures values are within specified range
- NonNegative - Ensures numeric values are not negative
- MaxLength - Ensures string length constraints
- That - Custom condition validation

### 4. Structured Logging

#### Using the Logger

```csharp
public class OrderProcessor
{
    private readonly IAppLogger<OrderProcessor> _logger;
    
    public OrderProcessor(IAppLogger<OrderProcessor> logger)
    {
        _logger = logger;
    }
    
    public async Task ProcessOrderAsync(Order order)
    {
        _logger.Info("Processing order {OrderId} for customer {CustomerId}", 
            order.Id, order.CustomerId);
        
        try
        {
            // Processing logic
            _logger.Info("Order {OrderId} processed successfully", order.Id);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to process order {OrderId}", order.Id);
            throw;
        }
    }
}
```

##### Logging Levels:
- Trace - Detailed debugging information
- Info - General information messages
- Warn - Warning messages for unexpected situations
- Error - Error messages with exceptions

### 5. Current User Service

Access current user context in your application layers:

```csharp
public class AuditService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<AuditLog> _auditRepository;
    
    public AuditService(ICurrentUserService currentUserService, IRepository<AuditLog> auditRepository)
    {
        _currentUserService = currentUserService;
        _auditRepository = auditRepository;
    }
    
    public async Task LogActionAsync(string action, CancellationToken ct = default)
    {
        if (_currentUserService.IsAuthenticated)
        {
            var auditLog = new AuditLog(
                action, 
                _currentUserService.UserId!);
                
            await _auditRepository.AddAsync(auditLog, ct);
        }
    }
}
```

#### Interface Properties:
- UserId - Current user identifier
- IsAuthenticated - Authentication status

##### Implementation Example

```csharp
public class HttpCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public HttpCurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    public bool IsAuthenticated => UserId != null;
}
```

### 6. MQTT Messaging

#### Publishing Messages

```csharp
public class DeviceService
{
    private readonly IMqttClientService _mqttClientService;
    
    public DeviceService(IMqttClientService mqttClientService)
    {
        _mqttClientService = mqttClientService;
    }
    
    public async Task SendDeviceCommandAsync(string deviceId, string command, CancellationToken ct = default)
    {
        var topic = $"devices/{deviceId}/commands";
        await _mqttClientService.PublishAsync(topic, command, qos: 1, ct: ct);
    }
}
```

##### MQQT Features:
- Async connection management
- Configurable QoS levels
- Retain flag support
- Connection state monitoring

##### MQTT Message DTO
```csharp
var message = new MqttMessage(
    topic: "devices/123/commands",
    payload: "{\"command\": \"restart\"}",
    retain: false,
    qos: 1
);
```

### 7. Result Pattern

Handle operation outcomes in a functional way:

```csharp
public class UserRegistrationService
{
    public Result<User> RegisterUser(string email, string password)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(email))
            return Result<User>.Failure("Email is required");
            
        if (string.IsNullOrWhiteSpace(password))
            return Result<User>.Failure("Password is required");
            
        // Business logic
        var user = new User(email, password);
        
        return Result<User>.Success(user);
    }
    
    public async Task<Result> ProcessRegistrationAsync(string email, string password)
    {
        var result = RegisterUser(email, password);
        
        if (!result.IsSuccess)
            return Result.Failure(result.Error!);
            
        // Persist user
        await _userRepository.AddAsync(result.Value!);
        await _unitOfWork.SaveChangesAsync();
        
        return Result.Success();
    }
}
```

#### Result Types:
- Result - For operations without return values
- Result<T> - For operations with return values

#### Key Properties:
- IsSuccess - Operation success status
- Error - Error message (if failed)
- Value - Return value (for Result<T>)

## 🔧 Setup and Configuration
### ASP.NET Core Configuration
```csharp
// Program.cs or Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    // Register building blocks
    services.AddScoped<ICurrentUserService, HttpCurrentUserService>();
    services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
    services.AddScoped<IUnitOfWork, UnitOfWork>();
    services.AddSingleton(typeof(IAppLogger<>), typeof(SerilogLogger<>));
    
    // Register your domain services
    services.AddScoped<ProductService>();
    services.AddScoped<OrderService>();
}

// Configure Serilog (if using SerilogLogger)
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
```

### Entity Framework Integration Example
```csharp
public class EfRepository<T> : IRepository<T> where T : BaseEntity
{
    private readonly ApplicationDbContext _context;
    
    public EfRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Set<T>().FindAsync(new object[] { id }, ct);
    }
    
    public IQueryable<T> Query(Expression<Func<T, bool>>? predicate = null)
    {
        return predicate != null 
            ? _context.Set<T>().Where(predicate)
            : _context.Set<T>();
    }
    
    public async Task AddAsync(T entity, CancellationToken ct = default)
    {
        await _context.Set<T>().AddAsync(entity, ct);
    }
    
    public async Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        _context.Set<T>().Update(entity);
        await Task.CompletedTask;
    }
    
    public async Task DeleteAsync(T entity, CancellationToken ct = default)
    {
        _context.Set<T>().Remove(entity);
        await Task.CompletedTask;
    }
}
```

### MQTT Client Implementation Example
```csharp
public class MqttNetClientService : IMqttClientService
{
    private readonly IMqttClient _mqttClient;
    private readonly MqttClientOptions _options;
    
    public MqttNetClientService(MqttClientOptions options)
    {
        _mqttClient = new MqttFactory().CreateMqttClient();
        _options = options;
    }
    
    public async Task ConnectAsync(CancellationToken ct = default)
    {
        await _mqttClient.ConnectAsync(_options, ct);
    }
    
    public async Task PublishAsync(string topic, string payload, bool retain = false, int qos = 1, CancellationToken ct = default)
    {
        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .WithRetainFlag(retain)
            .WithQualityOfServiceLevel((MQTTnet.Protocol.MqttQualityOfServiceLevel)qos)
            .Build();
            
        await _mqttClient.PublishAsync(message, ct);
    }
    
    public bool IsConnected => _mqttClient.IsConnected;
    
    public async ValueTask DisposeAsync()
    {
        if (_mqttClient.IsConnected)
        {
            await _mqttClient.DisconnectAsync();
        }
        _mqttClient.Dispose();
    }
}
```
## 🧪 Testing
### Mocking Dependencies
```csharp
public class ProductServiceTests
{
    [Fact]
    public async Task GetProduct_ReturnsProduct_WhenExists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var expectedProduct = new Product("Test Product", 99.99m);
        
        var mockRepository = new Mock<IRepository<Product>>();
        mockRepository.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProduct);
            
        var mockLogger = new Mock<IAppLogger<ProductService>>();
        var service = new ProductService(mockRepository.Object, mockLogger.Object);
        
        // Act
        var result = await service.GetProductAsync(productId);
        
        // Assert
        Assert.Equal(expectedProduct, result);
    }
    
    [Fact]
    public void CreateUser_ThrowsException_WhenEmailIsNull()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => new User(null!, "Password"));
    }
}
```

### Testing Domain Events
```csharp
[Fact]
public void ProductCreation_RaisesDomainEvent()
{
    // Arrange & Act
    var product = new Product("Test Product", 99.99m);
    
    // Assert
    Assert.Single(product.DomainEvents);
    Assert.IsType<ProductCreatedDomainEvent>(product.DomainEvents.First());
}
```

## 📋 Best Practices
### 1. Domain Entity Design
```csharp
// ✅ Good - Proper encapsulation
public class Order : BaseEntity
{
    private readonly List<OrderItem> _items = new();
    
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public decimal Total => _items.Sum(x => x.Subtotal);
    
    public void AddItem(Product product, int quantity)
    {
        Guard.NotNull(product, nameof(product));
        Guard.InRange(quantity, 1, 100, nameof(quantity));
        
        _items.Add(new OrderItem(product, quantity));
        SetModified();
    }
}

// ❌ Avoid - Public setters break encapsulation
public class BadOrder : BaseEntity
{
    public List<OrderItem> Items { get; set; } = new(); // Public setter
}
```

### 2. Guard Clause Usage
```csharp
// ✅ Good - Validate all inputs
public void ProcessOrder(Order order, string processorId)
{
    Guard.NotNull(order, nameof(order));
    Guard.NotNullOrWhiteSpace(processorId, nameof(processorId));
    Guard.That(order.Items.Any(), "Order must have items", nameof(order));
    
    // Business logic
}

// ❌ Avoid - Missing validation
public void ProcessOrder(Order order, string processorId)
{
    // No validation - potential null reference exceptions
    _logger.Info($"Processing order {order.Id}"); // order could be null!
}
```

### 3. Result Pattern for Operation Outcomes
```csharp
// ✅ Good - Clear success/failure handling
public Result<Order> CreateOrder(CreateOrderRequest request)
{
    var validationResult = ValidateRequest(request);
    if (!validationResult.IsSuccess)
        return Result<Order>.Failure(validationResult.Error);
    
    var order = new Order(request.CustomerId);
    // ... order creation logic
    
    return Result<Order>.Success(order);
}

// ❌ Avoid - Exceptions for control flow
public Order CreateOrder(CreateOrderRequest request)
{
    if (string.IsNullOrEmpty(request.CustomerId))
        throw new ArgumentException("Customer ID required"); // Use exceptions for exceptional cases only
    
    // ... order creation logic
    return order;
}
```

### 4. Proper Logging
```csharp
// ✅ Good - Structured logging with context
public async Task ProcessPaymentAsync(Payment payment)
{
    _logger.Info("Processing payment {PaymentId} for amount {Amount}", 
        payment.Id, payment.Amount);
    
    try
    {
        // Payment processing
        _logger.Info("Payment {PaymentId} processed successfully", payment.Id);
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "Failed to process payment {PaymentId}", payment.Id);
        throw;
    }
}

// ❌ Avoid - Unstructured logging
public async Task ProcessPaymentAsync(Payment payment)
{
    _logger.Info($"Processing payment {payment.Id}"); // String interpolation
    // No error handling
}
```

### 5. Domain Event Best Practices
```csharp
// ✅ Good - Rich domain events with all needed data
public record OrderShippedDomainEvent(
    DateTime OccurredOnUtc, 
    Guid OrderId, 
    string TrackingNumber, 
    DateTime EstimatedDelivery) : DomainEvent(OccurredOnUtc);

// Raise event in domain method
public void ShipOrder(string trackingNumber, DateTime estimatedDelivery)
{
    TrackingNumber = trackingNumber;
    Status = OrderStatus.Shipped;
    
    RaiseDomainEvent(new OrderShippedDomainEvent(
        DateTime.UtcNow, 
        Id, 
        trackingNumber, 
        estimatedDelivery));
        
    SetModified();
}
```

##  🔄 Extension Points
### Custom Domain Events

```csharp
public record OrderShippedDomainEvent(
    DateTime OccurredOnUtc, 
    Guid OrderId, 
    string TrackingNumber, 
    DateTime EstimatedDelivery) : DomainEvent(OccurredOnUtc);
```

### Custom Repository Methods
```csharp
public interface IOrderRepository : IRepository<Order>
{
    Task<List<Order>> GetOrdersByCustomerAsync(Guid customerId, CancellationToken ct = default);
    Task<Order?> GetOrderWithItemsAsync(Guid orderId, CancellationToken ct = default);
    Task<List<Order>> GetPendingOrdersAsync(CancellationToken ct = default);
}

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;
    
    public OrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    // Implement IRepository methods...
    
    public async Task<List<Order>> GetOrdersByCustomerAsync(Guid customerId, CancellationToken ct = default)
    {
        return await _context.Orders
            .Where(o => o.CustomerId == customerId)
            .ToListAsync(ct);
    }
}
```

### Custom Guard Clauses
```csharp
public static class CustomGuard
{
    public static string ValidEmail(string value, string paramName)
    {
        Guard.NotNullOrWhiteSpace(value, paramName);
        
        if (!value.Contains("@"))
            throw new ArgumentException("Invalid email format", paramName);
            
        return value;
    }
    
    public static string StrongPassword(string value, string paramName)
    {
        Guard.NotNullOrWhiteSpace(value, paramName);
        Guard.That(value.Length >= 8, "Password must be at least 8 characters", paramName);
        
        return value;
    }
}
```

## 🆘 Troubleshooting
### Common Issues
1. Domain events not being dispatched
   -  Ensure you have a domain event dispatcher implementation in your infrastructure layer
   - Call ClearDomainEvents() after dispatching events
   -  Check that events are actually being raised in domain methods
2. Audit fields not populated
   - Call SetCreated() when creating entities
   - Call SetModified() when updating entities  
   - Ensure these methods are called before saving changes
3. MQTT connection issues
     - Check broker configuration in your infrastructure layer
     - Ensure proper exception handling in connection logic
     - Verify network connectivity to MQTT broker
4. Repository query performance
     - Use Query() method for complex queries to leverage IQueryable
     - Consider implementing specification pattern for complex queries
     - Use eager loading for related entities when needed
5. Guard clause exceptions in production
     - Ensure all input validation happens at application boundaries
     - Use the Result pattern for business rule validation instead of Guards
     - Guards are for programming errors, not business rule violations

### Debugging Tips
```csharp
// Enable detailed logging for debugging
public class DebugService
{
    private readonly IAppLogger<DebugService> _logger;
    
    public DebugService(IAppLogger<DebugService> logger)
    {
        _logger = logger;
    }
    
    public void DebugDomainEvents(BaseEntity entity)
    {
        _logger.Info("Entity {EntityId} has {EventCount} domain events", 
            entity.Id, entity.DomainEvents.Count);
            
        foreach (var domainEvent in entity.DomainEvents)
        {
            _logger.Info("Domain event: {EventType} at {Timestamp}", 
                domainEvent.GetType().Name, domainEvent.OccurredOnUtc);
        }
    }
}
```

## 📚 Related Packages
This building blocks package is designed to work with:
- Entity Framework Core - for repository implementations
- Serilog - for structured logging
- MQTTnet - for MQTT messaging
- ASP.NET Core - for current user service and dependency injection

## 🤝 Contributing
Please read our contributing guidelines and code of conduct before submitting pull requests.

## 📄 License
This project is licensed under the MIT License - see the LICENSE file for details.

##
Version: 1.0.0
Compatibility: .NET 6.0+
Dependencies: Serilog, MQTTnet (in infrastructure layer)
