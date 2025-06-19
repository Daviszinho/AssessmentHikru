import React, { useState, useEffect, useRef } from 'react';
import PositionTable from './components/PositionsTable.tsx';
import type { Position } from './components/PositionsTable.tsx';
import reactLogo from './assets/react.svg';
import viteLogo from '/vite.svg';
import './App.css';

const API_URL = 'https://hikrutestapp.azurewebsites.net/api/positions';

const App: React.FC = () => {
  // Refs for focus management
  const mainContentRef = useRef<HTMLElement>(null);
  const skipToContentRef = useRef<HTMLAnchorElement>(null);
  const lastFocusedElement = useRef<HTMLElement | null>(null);
  
  // State management
  const [positions, setPositions] = useState<Position[]>([]);
  const [expandedIndex, setExpandedIndex] = useState<number | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Handle skip to content
  const handleSkipToContent = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && mainContentRef.current) {
      e.preventDefault();
      mainContentRef.current.focus();
    }
  };

  const fetchPositions = async () => {
    console.log('[App] Starting to fetch positions...');
    setIsLoading(true);
    setError(null);
    try {
      console.log('[App] Sending GET request to:', API_URL);
      const startTime = performance.now();
      const response = await fetch(API_URL, {
        method: 'GET',
        headers: {
          'Accept': 'application/json',
          'Content-Type': 'application/json',
          'Cache-Control': 'no-cache, no-store, must-revalidate',
          'Pragma': 'no-cache',
          'Expires': '0'
        },
        mode: 'cors',
        credentials: 'same-origin',
        cache: 'no-store'
      });
      
      const endTime = performance.now();
      console.log(`[App] Received response in ${(endTime - startTime).toFixed(2)}ms`);
      console.log('[App] Response status:', response.status, response.statusText);
      
      if (!response.ok) {
        const errorText = await response.text();
        console.error('[App] Error response:', errorText);
        throw new Error(`HTTP error! status: ${response.status} - ${errorText}`);
      }
      
      const data = await response.json();
      console.log('[App] Received data:', JSON.parse(JSON.stringify(data)));
      console.log(`[App] Setting ${data.length} positions to state`);
      setPositions(data);
      return data;
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Unknown error occurred';
      console.error('Error fetching positions:', errorMessage, err);
      setError(`Failed to load positions: ${errorMessage}`);
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchPositions();
  }, []);

  const handleAdd = async (newPosition: Position) => {
    try {
      const response = await fetch(API_URL, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(newPosition),
      });
      
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      
      const addedPosition = await response.json();
      setPositions(prev => [...prev, addedPosition]);
    } catch (err) {
      console.error('Error adding position:', err);
      setError('Failed to add position. Please try again.');
    }
  };

  const handleDelete = async (id: string) => {
    try {
      console.log('Deleting position with raw ID:', id);
      
      // Check if ID is provided and valid
      if (!id || id.trim() === '') {
        throw new Error('Position ID is required');
      }
      
      // Ensure the ID is a number since the backend expects an int
      const positionId = parseInt(id, 10);
      if (isNaN(positionId) || positionId <= 0) {
        throw new Error(`Invalid position ID: ${id}. Must be a positive number`);
      }

      console.log(`Deleting position with ID: ${positionId}`);
      const response = await fetch(`${API_URL}/${positionId}`, {
        method: 'DELETE',
        mode: 'cors', // Explicitly set CORS mode
        headers: {
          'Accept': 'application/json',
          'Content-Type': 'application/json',
          'Origin': window.location.origin // Send the origin header
        },
        credentials: 'same-origin' // Only send credentials for same-origin requests
      });
      
      console.log('Delete response status:', response.status);
      
      if (response.status === 200 || response.status === 204) { // 200 OK or 204 No Content for successful delete
        // Refresh the positions list from the server to ensure consistency
        await fetchPositions();
        // Reset expanded index if the deleted position was expanded
        if (expandedIndex !== null) setExpandedIndex(null);
        return true;
      } else if (response.status === 404) {
        throw new Error('Position not found');
      } else if (response.status === 501) {
        const errorData = await response.text();
        throw new Error(errorData || 'Delete operation not implemented on server');
      } else {
        const errorData = await response.text();
        throw new Error(errorData || `HTTP error! status: ${response.status}`);
      }
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to delete position';
      console.error('Error deleting position:', errorMessage, err);
      setError(`Failed to delete position: ${errorMessage}`);
      return false;
    }
  };

  const handleUpdate = async (id: string, updatedPosition: Position) => {
    try {
      // Convert string IDs to numbers and validate
      const recruiterId = Number(updatedPosition.recruiterId);
      const departmentId = Number(updatedPosition.departmentId);
      const budget = Number(updatedPosition.budget);

      // Validate number conversion
      if (isNaN(recruiterId) || isNaN(departmentId) || isNaN(budget)) {
        throw new Error('Invalid number format in position data');
      }

      // Format date if present
      const closingDate = updatedPosition.closingDate 
        ? new Date(updatedPosition.closingDate).toISOString().split('T')[0]
        : null;

      // Create a properly typed position object with all required fields
      const positionToUpdate = {
        ...updatedPosition,
        recruiterId,
        departmentId,
        budget,
        status: updatedPosition.status || 'draft', // Ensure status has a default value
        closingDate: closingDate || null
      };

      console.log('Sending update request:', { id, positionToUpdate });
      
      const response = await fetch(`${API_URL}/${id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json',
        },
        mode: 'cors',
        credentials: 'include',
        body: JSON.stringify(positionToUpdate),
      });
      
      const responseData = await response.json();
      console.log('Update response:', { status: response.status, data: responseData });
      
      if (!response.ok) {
        const errorMessage = responseData?.message || `HTTP error! status: ${response.status}`;
        throw new Error(errorMessage);
      }
      
      // Update the positions list with the updated position
      setPositions(prev => 
        prev.map(pos => pos.id === id ? { ...responseData } : pos)
      );
      
      return responseData;
    } catch (err) {
      console.error('Error updating position:', err);
      const errorMessage = err instanceof Error ? err.message : 'Failed to update position';
      setError(`Error: ${errorMessage}`);
      throw err; // Re-throw to be handled by the caller
    }
  };



  const handleExpand = (idx: number) => {
    setExpandedIndex(expandedIndex === idx ? null : idx);
  };

  // Set up focus management for modals/dialogs
  useEffect(() => {
    // Save the currently focused element when the component mounts
    lastFocusedElement.current = document.activeElement as HTMLElement;
    
    // Set focus to the main content area when the component mounts
    if (mainContentRef.current) {
      mainContentRef.current.focus();
    }

    // Cleanup function to restore focus when component unmounts
    return () => {
      if (lastFocusedElement.current) {
        lastFocusedElement.current.focus();
      }
    };
  }, []);

  return (
    <div className="app-container" role="application">
      {/* Skip to main content link - hidden until focused */}
      <a
        href="#main-content"
        ref={skipToContentRef}
        className="skip-to-content"
        onKeyDown={handleSkipToContent}
      >
        Skip to main content
      </a>

      <header className="app-header" role="banner">
        <h1>Welcome to Hikrutech - Full Stack Lead</h1>
        <h2>Full Stack Lead - Davis Peñaranda</h2>
        <nav aria-label="External links">
          <ul className="logo-list">
            <li>
              <a 
                href="https://www.hikrutech.com/" 
                target="_blank" 
                rel="noopener noreferrer"
                aria-label="Visit Hikrutech website"
              >
                <img 
                  src="https://cdn.prod.website-files.com/66c2cf5006b8d37eca26c0d2/66d030e05d6df2a0e0b5aabe_logo-footer.png" 
                  className="logo" 
                  alt="Hikrutech logo" 
                />
              </a>
            </li>
            <li>
              <a 
                href="https://vite.dev" 
                target="_blank" 
                rel="noopener noreferrer"
                aria-label="Learn about Vite"
              >
                <img 
                  src={viteLogo} 
                  className="logo" 
                  alt="Vite logo" 
                />
              </a>
            </li>
            <li>
              <a 
                href="https://react.dev" 
                target="_blank" 
                rel="noopener noreferrer"
                aria-label="Learn about React"
              >
                <img 
                  src={reactLogo} 
                  className="logo react" 
                  alt="React logo" 
                />
              </a>
            </li>
          </ul>
        </nav>
      </header>

      <main 
        id="main-content"
        ref={mainContentRef}
        role="main"
        tabIndex={-1}
        aria-labelledby="page-title"
        className={isLoading ? 'loading' : ''}
      >
        {error && (
          <div 
            role="alert" 
            aria-live="assertive"
            className="error-message"
          >
            {error}
          </div>
        )}

        {isLoading ? (
          <div 
            role="status" 
            aria-live="polite"
            aria-busy={isLoading}
          >
            <p>Loading positions...</p>
            <div className="loading-spinner" aria-hidden="true"></div>
          </div>
        ) : (
          <PositionTable 
            positions={positions}
            expandedIndex={expandedIndex}
            onExpand={handleExpand}
            onDelete={handleDelete}
            onUpdate={handleUpdate}
            onAdd={handleAdd}
          />
        )}
      </main>
    </div>
  );
}

export default App;
