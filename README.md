# EVDealerSeller

## Project Description
EVDealerSeller is a web application designed to manage electric vehicle dealership operations. It provides features for managing vehicles, customer orders, test drives, and more. The application is built using ASP.NET Core and follows a microservices architecture.

## Features
- Vehicle management
- Customer order processing
- Test drive scheduling
- Authentication and authorization using JWT
- Session management

## Technologies Used
- ASP.NET Core 8.0
- Entity Framework Core
- Docker
- JWT Authentication
- SignalR

## Account
- Staff Dealer: staff@gmail.com / Password: 123
- Manager Dealer: manager@gmail.com / Password: 123
  
## Setup Instructions
1. **Clone the repository**
   ```bash
   git clone <repository-url>
   ```
2. **Navigate to the project directory**
   ```bash
   cd EVDealerSales
   ```
3. **Build the Docker image**
   ```bash
   docker build -t evdealerseller .
   ```
4. **Run the application**
   ```bash
   docker-compose up
   ```

## Usage
- Access the application at `http://localhost:5000`
- Use the default credentials to log in and explore the features.

## Contributing
Contributions are welcome! Please fork the repository and submit a pull request for any improvements.

## License
This project is licensed under the MIT License.
