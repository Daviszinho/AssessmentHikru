// API Configuration
export const API_CONFIG = {
  // Production API URL (Azure)
  PRODUCTION_URL: 'https://hikrutestapp.azurewebsites.net/api/positions',
  
  // Development API URL (local backend)
  DEVELOPMENT_URL: 'http://localhost:5246/api/positions',
  
  // Alternative local ports
  LOCAL_3000: 'http://localhost:3000/api/positions',
  LOCAL_5000: 'http://localhost:5000/api/positions',
  LOCAL_7000: 'http://localhost:7000/api/positions',
  LOCAL_8080: 'http://localhost:8080/api/positions',
  
  // Get the appropriate URL based on environment
  getApiUrl(): string {
    // Check if VITE_API_URL is set in environment variables
    if (import.meta.env.VITE_API_URL) {
      return import.meta.env.VITE_API_URL;
    }
    
    // Check environment mode
    if (import.meta.env.MODE === 'development') {
      // In development, use local backend
      return this.DEVELOPMENT_URL;
    }
    
    // Fallback to production URL
    return this.PRODUCTION_URL;
  },
  
  // Method to manually set API URL
  setApiUrl(url: string): void {
    console.log('Setting API URL to:', url);
    // This would need to be implemented with a state management solution
    // For now, you can modify the return value of getApiUrl()
  }
};

// Export the API URL
export const API_URL = API_CONFIG.getApiUrl();

// Log the API URL being used (for debugging)
console.log('API URL configured:', API_URL);
console.log('Environment:', import.meta.env.MODE);
console.log('VITE_API_URL from env:', import.meta.env.VITE_API_URL);

// Export individual URLs for easy access
export const {
  PRODUCTION_URL,
  DEVELOPMENT_URL,
  LOCAL_3000,
  LOCAL_5000,
  LOCAL_7000,
  LOCAL_8080
} = API_CONFIG; 