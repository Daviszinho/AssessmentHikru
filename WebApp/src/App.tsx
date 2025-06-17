import React, { useState, useEffect } from 'react';
import PositionTable from './components/PositionsTable.tsx';
import type { Position } from './components/PositionsTable.tsx';
import reactLogo from './assets/react.svg';
import viteLogo from '/vite.svg';
import './App.css';

const API_URL = 'http://localhost:5246/api/positions';

function App() {
  const [positions, setPositions] = useState<Position[]>([]);
  const [expandedIndex, setExpandedIndex] = useState<number | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchPositions = async () => {
      try {
        console.log('Attempting to fetch positions from:', API_URL);
        const response = await fetch(API_URL, {
          method: 'GET',
          headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
          },
          mode: 'cors',
          credentials: 'same-origin'
        });
        
        console.log('Response status:', response.status, response.statusText);
        
        if (!response.ok) {
          const errorText = await response.text();
          console.error('Error response:', errorText);
          throw new Error(`HTTP error! status: ${response.status} - ${errorText}`);
        }
        
        const data = await response.json();
        console.log('Received data:', data);
        setPositions(data);
      } catch (err) {
        const errorMessage = err instanceof Error ? err.message : 'Unknown error occurred';
        console.error('Error fetching positions:', errorMessage, err);
        setError(`Failed to load positions: ${errorMessage}`);
      } finally {
        setIsLoading(false);
      }
    };

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
      const response = await fetch(`${API_URL}/${id}`, {
        method: 'DELETE',
      });
      
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      
      setPositions(prev => prev.filter(position => position.id !== id));
      if (expandedIndex !== null) setExpandedIndex(null);
    } catch (err) {
      console.error('Error deleting position:', err);
      setError('Failed to delete position. Please try again.');
    }
  };

  const handleUpdate = async (id: string, updatedPosition: Position) => {
    try {
      const response = await fetch(`${API_URL}/${id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(updatedPosition),
      });
      
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      
      const data = await response.json();
      setPositions(prev => 
        prev.map(pos => pos.id === id ? data : pos)
      );
    } catch (err) {
      console.error('Error updating position:', err);
      setError('Failed to update position. Please try again.');
    }
  };



  const handleExpand = (idx: number) => {
    setExpandedIndex(expandedIndex === idx ? null : idx);
  };

  return (
    <div>
      <div>
        <h1>Welcome to Hikrutech - Full Stack Lead</h1>
        <h2>Full Stack Lead - Davis Penaranda</h2>
        <a href="https://www.hikrutech.com/" target="_blank">
          <img src="https://cdn.prod.website-files.com/66c2cf5006b8d37eca26c0d2/66d030e05d6df2a0e0b5aabe_logo-footer.png" className="logo" alt="Vite logo" />
        </a>
        <a href="https://vite.dev" target="_blank">
          <img src={viteLogo} className="logo" alt="Vite logo" />
        </a>
        <a href="https://react.dev" target="_blank">
          <img src={reactLogo} className="logo react" alt="React logo" />
        </a>
      </div>
      {isLoading ? (
        <div>Loading positions...</div>
      ) : error ? (
        <div className="error">{error}</div>
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
    </div>
  );
}

export default App;
