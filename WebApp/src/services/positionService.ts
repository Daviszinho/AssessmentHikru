import type { Position } from '../types/position';
import { API_URL } from '../config/api';

export const fetchPositions = async (): Promise<Position[]> => {
  console.log('[PositionService] Fetching positions from:', API_URL);
  const response = await fetch(API_URL, {
    method: 'GET',
    headers: {
      'Accept': 'application/json',
      'Content-Type': 'application/json',
      'Cache-Control': 'no-cache, no-store, must-revalidate',
      'Pragma': 'no-cache'
    },
    mode: 'cors',
    cache: 'no-store'
  });

  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(`HTTP error! status: ${response.status} - ${errorText}`);
  }

  return response.json();
};

export const addPosition = async (position: Omit<Position, 'id'>): Promise<Position> => {
  const response = await fetch(API_URL, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(position)
  });

  if (!response.ok) {
    throw new Error(`Failed to add position: ${response.statusText}`);
  }

  return response.json();
};

export const deletePosition = async (id: string): Promise<boolean> => {
  const positionId = parseInt(id, 10);
  if (isNaN(positionId) || positionId <= 0) {
    throw new Error(`Invalid position ID: ${id}. Must be a positive number`);
  }

  const response = await fetch(`${API_URL}/${positionId}`, {
    method: 'DELETE',
    mode: 'cors',
    headers: {
      'Accept': 'application/json',
      'Content-Type': 'application/json',
      'Origin': window.location.origin
    },
    credentials: 'same-origin'
  });

  if (response.status === 404) {
    throw new Error('Position not found');
  }

  if (!response.ok) {
    const errorData = await response.text();
    throw new Error(errorData || `HTTP error! status: ${response.status}`);
  }

  return true;
};

export const updatePosition = async (id: string, updatedPosition: Position): Promise<Position> => {
  const response = await fetch(`${API_URL}/${id}`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
      'Accept': 'application/json',
    },
    mode: 'cors',
    credentials: 'include',
    body: JSON.stringify(updatedPosition),
  });

  if (!response.ok) {
    const errorData = await response.text();
    throw new Error(errorData || `HTTP error! status: ${response.status}`);
  }

  return response.json();
};
