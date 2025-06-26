import { useState, useEffect, useCallback } from 'react';
import type { Position } from '../types/position';
import * as positionService from '../services/positionService';

export const usePositions = () => {
  const [positions, setPositions] = useState<Position[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [expandedIndex, setExpandedIndex] = useState<number | null>(null);

  const loadPositions = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      const data = await positionService.fetchPositions();
      setPositions(data);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to load positions';
      setError(errorMessage);
      console.error('Error loading positions:', err);
    } finally {
      setIsLoading(false);
    }
  }, []);

  const addPosition = async (newPosition: Omit<Position, 'id'>) => {
    try {
      const addedPosition = await positionService.addPosition(newPosition);
      setPositions(prev => [...prev, addedPosition]);
      return addedPosition;
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to add position';
      setError(errorMessage);
      console.error('Error adding position:', err);
      throw err;
    }
  };

  const deletePosition = async (id: string) => {
    try {
      const success = await positionService.deletePosition(id);
      if (success) {
        await loadPositions();
        if (expandedIndex !== null) setExpandedIndex(null);
      }
      return success;
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to delete position';
      setError(errorMessage);
      console.error('Error deleting position:', err);
      throw err;
    }
  };

  const updatePosition = async (id: string, updatedData: Position) => {
    try {
      const updatedPosition = await positionService.updatePosition(id, updatedData);
      setPositions(prev => 
        prev.map(pos => pos.id === id ? { ...updatedPosition } : pos)
      );
      return updatedPosition;
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to update position';
      setError(errorMessage);
      console.error('Error updating position:', err);
      throw err;
    }
  };

  const toggleExpand = (index: number) => {
    setExpandedIndex(expandedIndex === index ? null : index);
  };

  useEffect(() => {
    loadPositions();
  }, [loadPositions]);

  return {
    positions,
    isLoading,
    error,
    expandedIndex,
    addPosition,
    deletePosition,
    updatePosition,
    toggleExpand,
    refreshPositions: loadPositions,
  };
};
