import { useState, useCallback } from 'react';
import type { Position } from '../types/position';
import { POSITION_CONSTANTS } from '../constants/positionConstants';

const defaultNewPosition: Position = {
  title: '',
  description: '',
  location: '',
  status: POSITION_CONSTANTS.STATUS.DRAFT,
  recruiterId: '',
  departmentId: '',
  budget: 0,
  closingDate: '',
};

export const usePositionForm = () => {
  const [showAddForm, setShowAddForm] = useState(false);
  const [newPosition, setNewPosition] = useState<Position>(defaultNewPosition);
  const [editIndex, setEditIndex] = useState<number | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleInputChange = useCallback((e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value, type } = e.target;
    
    setNewPosition(prev => {
      let newValue: string | number = value;
      
      if (type === 'number') {
        newValue = value === '' ? '' : Number(value);
      } else if (type === 'date') {
        newValue = value || '';
      }
      
      return {
        ...prev,
        [name]: newValue,
      };
    });
  }, []);

  const resetForm = useCallback(() => {
    setNewPosition(defaultNewPosition);
    setError(null);
    setEditIndex(null);
  }, []);

  const showAddFormHandler = useCallback(() => {
    setShowAddForm(true);
    resetForm();
  }, [resetForm]);

  const hideForm = useCallback(() => {
    setShowAddForm(false);
    setError(null);
  }, []);

  const showEditForm = useCallback((position: Position, index: number) => {
    const formattedPosition = {
      ...position,
      closingDate: position.closingDate 
        ? new Date(position.closingDate).toISOString().split('T')[0]
        : ''
    };
    
    setShowAddForm(true);
    setEditIndex(index);
    setNewPosition(formattedPosition);
    setError(null);
  }, []);

  const validateForm = useCallback((position: Position): string | null => {
    if (!position.title.trim()) {
      return 'Title is required.';
    }
    if (!position.description.trim()) {
      return 'Description is required.';
    }
    if (!position.location.trim()) {
      return 'Location is required.';
    }
    if (!position.recruiterId) {
      return 'Recruiter is required.';
    }
    if (!position.departmentId) {
      return 'Department is required.';
    }
    if (!position.budget && position.budget !== 0) {
      return 'Budget is required.';
    }
    if (position.title.length > POSITION_CONSTANTS.VALIDATION.MAX_TITLE_LENGTH) {
      return `Title must be at most ${POSITION_CONSTANTS.VALIDATION.MAX_TITLE_LENGTH} characters.`;
    }
    if (position.description.length > POSITION_CONSTANTS.VALIDATION.MAX_DESCRIPTION_LENGTH) {
      return `Description must be at most ${POSITION_CONSTANTS.VALIDATION.MAX_DESCRIPTION_LENGTH} characters.`;
    }
    
    const recruiterId = Number(position.recruiterId);
    const departmentId = Number(position.departmentId);
    const budget = Number(position.budget);
    
    if (isNaN(recruiterId) || recruiterId <= 0) {
      return 'Recruiter ID must be a valid positive number';
    }
    
    if (isNaN(departmentId) || departmentId <= 0) {
      return 'Department ID must be a valid positive number';
    }
    
    if (isNaN(budget) || budget < 0) {
      return 'Budget must be a valid non-negative number';
    }
    
    return null;
  }, []);

  const formatDateForBackend = useCallback((dateString?: string | null): string | undefined => {
    if (!dateString) return undefined;
    try {
      const date = new Date(dateString);
      if (isNaN(date.getTime())) return undefined;
      return date.toISOString().split('T')[0];
    } catch (e) {
      console.warn('Invalid date format:', dateString);
      return undefined;
    }
  }, []);

  return {
    showAddForm,
    newPosition,
    editIndex,
    error,
    isDeleting,
    isSubmitting,
    handleInputChange,
    resetForm,
    showAddFormHandler,
    hideForm,
    showEditForm,
    validateForm,
    formatDateForBackend,
    setError,
    setIsDeleting,
    setIsSubmitting,
    setNewPosition
  };
}; 