#!/bin/bash

# Setup script for WebApp environment variables

echo "Setting up environment variables for WebApp..."

# Create .env.local file if it doesn't exist
if [ ! -f ".env.local" ]; then
    echo "Creating .env.local file..."
    cat > .env.local << EOF
# API Configuration
VITE_API_URL=https://hikrutestapp.azurewebsites.net/api/positions

# Development settings
VITE_DEV_MODE=true
EOF
    echo "✅ .env.local file created successfully!"
else
    echo "⚠️  .env.local file already exists. Skipping creation."
fi

# Display current configuration
echo ""
echo "Current API Configuration:"
echo "VITE_API_URL: ${VITE_API_URL:-'Not set in environment'}"
echo ""

# Check if the API URL is accessible
echo "Testing API connectivity..."
if command -v curl &> /dev/null; then
    API_URL="${VITE_API_URL:-https://hikrutestapp.azurewebsites.net/api/positions}"
    echo "Testing: $API_URL"
    
    # Test with curl
    if curl -s -o /dev/null -w "%{http_code}" "$API_URL" | grep -q "200\|204"; then
        echo "✅ API is accessible!"
    else
        echo "⚠️  API might not be accessible. This could be due to CORS or server issues."
        echo "   You may need to run the backend server locally or check CORS configuration."
    fi
else
    echo "⚠️  curl not available. Skipping API connectivity test."
fi

echo ""
echo "Setup complete! You can now run:"
echo "  npm run dev"
echo ""
echo "If you encounter CORS issues, make sure:"
echo "1. The backend server is running"
echo "2. CORS is properly configured on the backend"
echo "3. You're using the correct API URL" 