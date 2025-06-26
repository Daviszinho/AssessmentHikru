import React, { useCallback, useMemo } from 'react';
import type { PositionTableProps, Position } from '../types/position';
import { POSITION_CONSTANTS } from '../constants/positionConstants';
import { usePositionForm } from '../hooks/usePositionForm';
import PositionRow from './PositionRow';
import PositionForm from './PositionForm';
import ErrorBoundary from './ErrorBoundary';
import './PositionsTable.css';

const PositionTable: React.FC<PositionTableProps> = React.memo(({
  positions,
  expandedIndex,
  onExpand,
  onDelete,
  onUpdate,
  onAdd,
}) => {
  const {
    showAddForm,
    newPosition,
    editIndex,
    error,
    isDeleting,
    isSubmitting,
    handleInputChange,
    showAddFormHandler,
    hideForm,
    showEditForm,
    validateForm,
    formatDateForBackend,
    setError,
    setIsDeleting,
    setIsSubmitting
  } = usePositionForm();

  // Memoize the context info to prevent unnecessary re-renders
  const contextInfo = useMemo(() => (
    <div style={POSITION_CONSTANTS.STYLES.CONTAINER}>
      <div><strong>Current Context:</strong></div>
      <div>Recruiter ID: 1</div>
      <div>Department ID: 1</div>
    </div>
  ), []);

  // Memoize the handleDeleteClick function
  const handleDeleteClick = useCallback(async (id: string, title: string): Promise<void> => {
    console.log('Delete button clicked', { id, title });
    const confirmed = window.confirm(`Are you sure you want to delete the position "${title}"?`);
    console.log('User confirmed delete:', confirmed);
    
    if (confirmed) {
      try {
        console.log('Calling onDelete with ID:', id);
        await onDelete(id);
        console.log('onDelete completed successfully');
      } catch (error) {
        console.error('Error in handleDeleteClick:', error);
        throw error;
      }
    }
  }, [onDelete]);

  // Memoize the handleEditClick function
  const handleEditClick = useCallback((position: Position, index: number) => {
    showEditForm(position, index);
  }, [showEditForm]);

  // Memoize the handleAddConfirm function
  const handleAddConfirm = useCallback(async () => {
    const validationError = validateForm(newPosition);
    if (validationError) {
      setError(validationError);
      return;
    }
    
    // Convert string IDs to numbers and validate
    const recruiterId = Number(newPosition.recruiterId);
    const departmentId = Number(newPosition.departmentId);
    const budget = Number(newPosition.budget);
    
    // Create position data with proper types and formatted date
    const positionData = {
      ...newPosition,
      recruiterId,
      departmentId,
      budget,
      status: newPosition.status || POSITION_CONSTANTS.STATUS.DRAFT,
      closingDate: formatDateForBackend(newPosition.closingDate)
    };
    
    console.log('Prepared position data for update:', positionData);
    
    setError(null);
    setIsSubmitting(true);
    
    try {
      if (editIndex !== null) {
        // Update existing position
        const positionToUpdate = positions[editIndex];
        if (positionToUpdate.id) {
          console.log('Updating position:', { id: positionToUpdate.id, position: positionData });
          await onUpdate(positionToUpdate.id, {
            ...positionData,
            id: positionToUpdate.id,
            positionId: positionToUpdate.positionId,
          });
          hideForm();
        } else {
          throw new Error('Cannot update: Position ID is missing');
        }
      } else if (onAdd) {
        // Add new position
        console.log('Adding new position:', positionData);
        await onAdd({
          ...positionData,
          closingDate: newPosition.closingDate?.trim() || undefined,
        });
        hideForm();
      }
    } catch (error) {
      console.error('Error saving position:', error);
      const errorMessage = error instanceof Error ? error.message : 'An error occurred while saving the position';
      setError(errorMessage);
    } finally {
      setIsSubmitting(false);
    }
  }, [newPosition, editIndex, positions, onUpdate, onAdd, validateForm, formatDateForBackend, setError, setIsSubmitting, hideForm]);

  // Memoize the table rows to prevent unnecessary re-renders
  const tableRows = useMemo(() => (
    positions.map((pos, idx) => (
      <PositionRow
        key={pos.id || `position-${idx}`}
        position={pos}
        index={idx}
        isExpanded={expandedIndex === idx}
        isDeleting={isDeleting}
        onExpand={onExpand}
        onEdit={handleEditClick}
        onDelete={handleDeleteClick}
      />
    ))
  ), [positions, expandedIndex, isDeleting, onExpand, handleEditClick, handleDeleteClick]);

  // Debug log the positions data
  console.log('Positions data:', positions);

  return (
    <ErrorBoundary>
      <div>
        {contextInfo}
        <table className="positions-table">
          <thead>
            <tr>
              <th scope="col">Title</th>
              <th scope="col">Description</th>
              <th scope="col">Actions</th>
            </tr>
          </thead>
          <tbody>
            {tableRows}
          </tbody>
        </table>
        <div style={{ marginTop: 16 }}>
          {!showAddForm ? (
            <button 
              className="action-button"
              onClick={showAddFormHandler}
              aria-label="Add new position"
            >
              [Add]
            </button>
          ) : (
            <PositionForm
              position={newPosition}
              isSubmitting={isSubmitting}
              error={error}
              onInputChange={handleInputChange}
              onSubmit={handleAddConfirm}
              onCancel={hideForm}
              isEdit={editIndex !== null}
            />
          )}
        </div>
      </div>
    </ErrorBoundary>
  );
});

PositionTable.displayName = 'PositionTable';

export default PositionTable;
