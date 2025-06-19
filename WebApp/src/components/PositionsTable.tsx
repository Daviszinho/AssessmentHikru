import React, { useState } from 'react';

export type Status = 'draft' | 'open' | 'closed' | 'archived';

export interface Position {
  id?: string; // Keep for backward compatibility
  positionId?: number | string; // Add positionId to match backend
  title: string;
  description: string;
  location: string;
  status: Status;
  recruiterId: number | string; // Can be number or string for form handling
  departmentId: number | string; // Can be number or string for form handling
  budget: number;
  closingDate?: string | null; // Allow null for form handling
}

interface PositionTableProps {
  positions: Position[];
  expandedIndex: number | null;
  onExpand: (idx: number) => void;
  onDelete: (id: string) => void;
  onUpdate: (id: string, position: Position) => void;
  onAdd?: (position: Position) => void;
}

const defaultNewPosition: Position = {
  title: '',
  description: '',
  location: '',
  status: 'draft',
  recruiterId: '',
  departmentId: '',
  budget: 0,
  closingDate: '',
};

const PositionTable: React.FC<PositionTableProps> = ({
  positions,
  expandedIndex,
  onExpand,
  onDelete,
  onUpdate,
  onAdd,
}) => {
  const [showAddForm, setShowAddForm] = useState(false);
  const [newPosition, setNewPosition] = useState<Position>(defaultNewPosition);
  const [editIndex, setEditIndex] = useState<number | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value, type } = e.target;
    
    setNewPosition(prev => {
      // Handle different input types
      let newValue: string | number = value;
      
      if (type === 'number') {
        newValue = value === '' ? '' : Number(value);
      } else if (type === 'date') {
        // For date inputs, we want to keep the raw string value
        // but ensure it's in the correct format for the date input
        newValue = value || '';
      }
      
      return {
        ...prev,
        [name]: newValue,
      };
    });
  };

  const handleAddClick = () => {
    setShowAddForm(true);
    setEditIndex(null);
    setNewPosition(defaultNewPosition);
    setError(null);
  };

  const handleEditClick = (idx: number) => {
    const positionToEdit = positions[idx];
    
    // Format the date for the date input (YYYY-MM-DD)
    const formattedPosition = {
      ...positionToEdit,
      closingDate: positionToEdit.closingDate 
        ? new Date(positionToEdit.closingDate).toISOString().split('T')[0]
        : ''
    };
    
    setShowAddForm(true);
    setEditIndex(idx);
    setNewPosition(formattedPosition);
    setError(null);
  };

  const handleDeleteClick = async (id: string, title: string): Promise<void> => {
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
        // Re-throw to ensure parent component handles the error
        throw error;
      }
    }
  };

  const handleAddConfirm = async () => {
    // Validation
    // Check required fields
    if (!newPosition.title.trim()) {
      setError('Title is required.');
      return;
    }
    if (!newPosition.description.trim()) {
      setError('Description is required.');
      return;
    }
    if (!newPosition.location.trim()) {
      setError('Location is required.');
      return;
    }
    if (!newPosition.recruiterId) {
      setError('Recruiter is required.');
      return;
    }
    if (!newPosition.departmentId) {
      setError('Department is required.');
      return;
    }
    if (!newPosition.budget && newPosition.budget !== 0) {
      setError('Budget is required.');
      return;
    }
    if (newPosition.title.length > 100) {
      setError('Title must be at most 100 characters.');
      return;
    }
    if (newPosition.description.length > 1000) {
      setError('Description must be at most 1000 characters.');
      return;
    }
    
    // Convert string IDs to numbers and validate
    const recruiterId = Number(newPosition.recruiterId);
    const departmentId = Number(newPosition.departmentId);
    const budget = Number(newPosition.budget);
    
    // Validate number conversion
    if (isNaN(recruiterId) || recruiterId <= 0) {
      setError('Recruiter ID must be a valid positive number');
      return;
    }
    
    if (isNaN(departmentId) || departmentId <= 0) {
      setError('Department ID must be a valid positive number');
      return;
    }
    
    if (isNaN(budget) || budget < 0) {
      setError('Budget must be a valid non-negative number');
      return;
    }
    
    // Format date to YYYY-MM-DD for the backend
    const formatDateForBackend = (dateString?: string | null): string | undefined => {
      if (!dateString) return undefined;
      try {
        const date = new Date(dateString);
        // Check if date is valid
        if (isNaN(date.getTime())) return undefined;
        return date.toISOString().split('T')[0];
      } catch (e) {
        console.warn('Invalid date format:', dateString);
        return undefined;
      }
    };

    // Create position data with proper types and formatted date
    const positionData = {
      ...newPosition,
      recruiterId,
      departmentId,
      budget,
      status: newPosition.status || 'draft', // Ensure status has a default value
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
            id: positionToUpdate.id, // Ensure ID is included
            positionId: positionToUpdate.positionId, // Preserve positionId if it exists
          });
          setShowAddForm(false);
          setEditIndex(null);
          setNewPosition(defaultNewPosition); // Reset form
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
        setShowAddForm(false);
        setNewPosition(defaultNewPosition); // Reset form
      }
    } catch (error) {
      console.error('Error saving position:', error);
      const errorMessage = error instanceof Error ? error.message : 'An error occurred while saving the position';
      setError(errorMessage);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleAddCancel = () => {
    setShowAddForm(false);
    setError(null);
  };

  // Debug log the positions data
  console.log('Positions data:', positions);

  return (
    <div>
      <table style={{ width: '100%', borderCollapse: 'collapse' }}>
        <thead>
          <tr>
            <th style={{ border: '1px solid #ccc' }}>Title</th>
            <th style={{ border: '1px solid #ccc' }}>Description</th>
            <th style={{ border: '1px solid #ccc' }}>Actions</th>
          </tr>
        </thead>
        <tbody>
          {positions.map((pos, idx) => (
            <React.Fragment key={pos.id || `position-${idx}`}>
              <tr 
                onClick={(e) => {
                  // Only trigger expand if the click was not on a button or other interactive element
                  const target = e.target as HTMLElement;
                  if (target.tagName !== 'BUTTON' && !target.closest('button')) {
                    onExpand(idx);
                  }
                }}
                style={{ cursor: 'pointer' }}
              >
                <td
                  style={{ border: '1px solid #ccc', color: '#1976d2' }}
                  title="Click to show details"
                >
                  {pos.title}
                </td>
                <td style={{ border: '1px solid #ccc' }}>{pos.description}</td>
                <td style={{ border: '1px solid #ccc', textAlign: 'center', whiteSpace: 'nowrap' }}>
                  <button
                    style={{
                      backgroundColor: '#4CAF50',
                      color: 'white',
                      border: 'none',
                      padding: '5px 10px',
                      margin: '0 5px',
                      borderRadius: '4px',
                      cursor: 'pointer'
                    }}
                    onClick={() => handleEditClick(idx)}
                  >
                    [Edit]
                  </button>
                  <button 
                    className="delete-button"
                    onClick={(e) => {
                      console.log('Delete button clicked - start');
                      e.preventDefault();
                      e.stopPropagation();
                      
                      console.log('Position data:', pos);
                      
                      // Check for both positionId and id for backward compatibility
                      const positionId = pos.positionId || pos.id;
                      if (!positionId || isDeleting) {
                        console.error('Cannot delete: Missing position ID or already deleting', { position: pos });
                        return;
                      }
                      
                      // Use void to explicitly ignore the promise returned by the async function
                      void (async () => {
                        try {
                          setIsDeleting(true);
                          const idToDelete = positionId.toString();
                          if (!idToDelete) {
                            throw new Error('Position ID is missing or invalid');
                          }
                          console.log('Calling handleDeleteClick with ID:', idToDelete);
                          await handleDeleteClick(idToDelete, pos.title);
                          console.log('Delete completed');
                        } catch (err) {
                          console.error('Delete failed:', err);
                        } finally {
                          setIsDeleting(false);
                        }
                      })();
                    }}
                    disabled={!pos.positionId && !pos.id || isDeleting}
                    style={{
                      // Inline styles are now handled by the CSS class
                    }}
                  >
                    [Delete]
                  </button>
                </td>
              </tr>
              {expandedIndex === idx && (
                <tr key={`expanded-${pos.id || `position-${idx}`}`}>
                  <td colSpan={3} style={{ border: '1px solid #ccc', background: '#c0c0c0', color: '#333' }}>
                    <div><strong>Location:</strong> {pos.location}</div>
                    <div><strong>Status:</strong> {pos.status}</div>
                    <div><strong>RecruiterId:</strong> {pos.recruiterId}</div>
                    <div><strong>DepartmentId:</strong> {pos.departmentId}</div>
                    <div><strong>Budget:</strong> {pos.budget}</div>
                    <div><strong>Closing date:</strong> {pos.closingDate || '-'}</div>
                  </td>
                </tr>
              )}
            </React.Fragment>
          ))}
        </tbody>
      </table>
      <div style={{ marginTop: 16 }}>
        {!showAddForm ? (
          <button onClick={handleAddClick}>[Add]</button>
        ) : (
          <div style={{ background: '#f5f5f5', padding: 16, borderRadius: 8, marginTop: 8, color: '#333' }}>
            <div>
              <input
                name="title"
                placeholder="Title*"
                maxLength={100}
                value={newPosition.title}
                onChange={handleInputChange}
                style={{ marginRight: 8 }}
              />
              <input
                name="description"
                placeholder="Description*"
                maxLength={1000}
                value={newPosition.description}
                onChange={handleInputChange}
                style={{ marginRight: 8 }}
              />
              <input
                name="location"
                placeholder="Location*"
                value={newPosition.location}
                onChange={handleInputChange}
                style={{ marginRight: 8 }}
              />
              <select
                name="status"
                value={newPosition.status}
                onChange={handleInputChange}
                style={{ marginRight: 8 }}
              >
                <option value="draft">draft</option>
                <option value="open">open</option>
                <option value="closed">closed</option>
                <option value="archived">archived</option>
              </select>
              <input
                name="recruiterId"
                placeholder="RecruiterId*"
                value={newPosition.recruiterId}
                onChange={handleInputChange}
                style={{ marginRight: 8 }}
              />
              <input
                name="departmentId"
                placeholder="DepartmentId*"
                value={newPosition.departmentId}
                onChange={handleInputChange}
                style={{ marginRight: 8 }}
              />
              <input
                name="budget"
                type="number"
                placeholder="Budget*"
                value={newPosition.budget}
                onChange={handleInputChange}
                style={{ marginRight: 8, width: 80 }}
              />
              <input
                name="closingDate"
                type="date"
                placeholder="Closing date"
                value={newPosition.closingDate || ''}
                onChange={handleInputChange}
                style={{ marginRight: 8 }}
              />
              <button
                style={{
                  backgroundColor: '#4CAF50',
                  color: 'white',
                  border: 'none',
                  padding: '6px 16px',
                  marginRight: 8,
                  borderRadius: 4,
                  cursor: isSubmitting ? 'not-allowed' : 'pointer',
                  opacity: isSubmitting ? 0.7 : 1
                }}
                onClick={handleAddConfirm}
                disabled={isSubmitting}
              >
                {isSubmitting ? 'Saving...' : (editIndex !== null ? '[Update]' : 'Add')}
              </button>
              <button
                style={{
                  backgroundColor: 'transparent',
                  border: '1px solid #f44336',
                  color: '#f44336',
                  padding: '4px 12px',
                  borderRadius: 4,
                  cursor: 'pointer'
                }}
                onClick={handleAddCancel}
              >
                [Cancel]
              </button>
            </div>
            {error && <div style={{ color: 'red', marginTop: 8 }}>{error}</div>}
          </div>
        )}
      </div>
    </div>
  );
};

export default PositionTable;
