# AWS Server Creator

AWS Server Creator is a web application built with ASP.NET Core (Razor Pages and Web API) that allows users to easily create, configure, and manage Amazon EC2 servers through a user-friendly interface. The application integrates with AWS SDK to provide automated server creation, management, and control directly from your browser.

## Features
- **Create EC2 Instances:** Launch new AWS EC2 instances with standard or custom configurations.
- **Server Management:** Start, stop, and monitor your instances from the web UI.
- **AWS Credentials:** Securely input and validate AWS Access Key and Secret Key.
- **SSH Key Management:** Create new key pairs, use existing ones, or upload your own public key.
- **Responsive UI:** Modern, responsive design with Bootstrap and FontAwesome icons.
- **Error Handling:** Informative error and success messages for all major actions.

## Technologies Used
- **.NET 8.0**
- **ASP.NET Core (Razor Pages & Web API)**
- **AWS SDK for .NET (EC2 & Core)**
- **Bootstrap 5**
- **JavaScript (custom logic in `wwwroot/js/aws-server-creator.js`)
- **HTML/CSS**

## Project Structure
```
AWS_SERVER_CREATOR/
├── Controllers/         # API Controllers (e.g., ServersController)
├── Models/              # Data models (e.g., AwsConfiguration, ServerActionRequest)
├── Pages/               # Razor Pages (UI)
├── Services/            # AWS EC2 Service logic
├── wwwroot/             # Static files (CSS, JS, images)
├── Program.cs           # Application entry point
├── AWS_SERVER_CREATOR.csproj # Project file
├── appsettings.json     # Configuration
```

## Getting Started
### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- AWS account with permissions to manage EC2

### Setup
1. **Clone the repository:**
   ```bash
   git clone <your-repo-url>
   cd AWS_SERVER_CREATOR
   ```
2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```
3. **Build the project:**
   ```bash
   dotnet build
   ```
4. **Run the application:**
   ```bash
   dotnet run
   ```
5. **Open your browser:**
   Navigate to `https://localhost:5001` or the port specified in the console output.

### Configuration
- Edit `appsettings.json` for custom configuration.
- AWS credentials are provided via the web UI and are not stored on the server.

## Usage
1. **Enter AWS Credentials:**
   - Access Key ID
   - Secret Access Key
   - Region
2. **Choose Server Configuration:**
   - Standard (predefined) or custom instance types and AMIs
3. **SSH Key Pair:**
   - Create new, use existing, or upload your public key
4. **Create and Manage Servers:**
   - Use the dashboard to start/stop instances and view server details

## Dependencies
- `AWSSDK.EC2` (v3.7.300)
- `AWSSDK.Core` (v3.7.300)
- `Microsoft.AspNetCore.Http` (v2.2.2)

## Security
- AWS credentials are never stored on the server or in logs.
- HTTPS is enforced in production mode.

## License
This project is licensed under the MIT License.

## Acknowledgements
- [AWS SDK for .NET](https://github.com/aws/aws-sdk-net)
- [Bootstrap](https://getbootstrap.com/)
- [FontAwesome](https://fontawesome.com/)