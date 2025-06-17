import React, { useState } from 'react';

export type Status = 'draft' | 'open' | 'closed' | 'archived';

export interface Position {
  id?: string;
  title: string;
  description: string;
  location: string;
  status: Status;
  recruiterId: string;
  departmentId: string;
  budget: number;
  closingDate?: string;
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

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setNewPosition(prev => ({
      ...prev,
      [name]: name === 'budget' ? Number(value) : value,
    }));
  };

  const handleAddClick = () => {
    setShowAddForm(true);
    setEditIndex(null);
    setNewPosition(defaultNewPosition);
    setError(null);
  };

  const handleEditClick = (idx: number) => {
    setShowAddForm(true);
    setEditIndex(idx);
    setNewPosition(positions[idx]);
    setError(null);
  };

  const handleAddConfirm = () => {
    // Validation
    if (
      !newPosition.title.trim() ||
      !newPosition.description.trim() ||
      !newPosition.location.trim() ||
      !newPosition.recruiterId.trim() ||
      !newPosition.departmentId.trim() ||
      !newPosition.budget
    ) {
      setError('Please fill all required fields.');
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
    setError(null);
    
    if (editIndex !== null) {
      const positionToUpdate = positions[editIndex];
      if (positionToUpdate.id) {
        onUpdate(positionToUpdate.id, newPosition);
        setShowAddForm(false);
        setEditIndex(null);
      } else {
        setError('Cannot update: Position ID is missing');
      }
    } else if (onAdd) {
      onAdd({
        ...newPosition,
        closingDate: newPosition.closingDate?.trim() || undefined,
      });
      setShowAddForm(false);
    }
  };

  const handleAddCancel = () => {
    setShowAddForm(false);
    setError(null);
  };

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
              <tr>
                <td
                  style={{ border: '1px solid #ccc', cursor: 'pointer', color: '#1976d2' }}
                  onClick={() => onExpand(idx)}
                  title="Click to show details"
                >
                  {pos.title}
                </td>
                <td style={{ border: '1px solid #ccc' }}>{pos.description}</td>
                <td style={{ border: '1px solid #ccc', textAlign: 'center', whiteSpace: 'nowrap' }}>
                  <button
                    style={{
                      backgroundColor: 'transparent',
                      border: '1px solid #ccc',
                      padding: '4px 8px',
                      marginRight: 8,
                      cursor: 'pointer'
                    }}
                    onClick={() => handleEditClick(idx)}
                  >
                    [Edit]
                  </button>
                  <button 
                    onClick={(e) => {
                      e.stopPropagation();
                      onUpdate(pos.id!, pos);
                    }}
                    disabled={!pos.id}
                    style={{
                      backgroundColor: '#4CAF50',
                      color: 'white',
                      border: 'none',
                      padding: '5px 10px',
                      margin: '0 5px',
                      borderRadius: '4px',
                      cursor: pos.id ? 'pointer' : 'not-allowed',
                      opacity: pos.id ? 1 : 0.5
                    }}
                  >
                    Update
                  </button>
                  <button 
                    onClick={(e) => {
                      e.stopPropagation();
                      onDelete(pos.id!);
                    }}
                    disabled={!pos.id}
                    style={{
                      backgroundColor: '#f44336',
                      color: 'white',
                      border: 'none',
                      padding: '5px 10px',
                      margin: '0 5px',
                      borderRadius: '4px',
                      cursor: pos.id ? 'pointer' : 'not-allowed',
                      opacity: pos.id ? 1 : 0.5
                    }}
                  >
                    Delete
                  </button>
                </td>
              </tr>
              {expandedIndex === idx && (
                <tr key={`expanded-${pos.id || `position-${idx}`}`}>
                  <td colSpan={3} style={{ border: '1px solid #ccc', background: '#1a1a1a' }}>
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
          <div style={{ background: '#222', padding: 16, borderRadius: 8, marginTop: 8 }}>
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
                value={newPosition.closingDate}
                onChange={handleInputChange}
                style={{ marginRight: 8 }}
              />
              <button
                style={{
                  backgroundColor: 'transparent',
                  border: '1px solid #4CAF50',
                  color: '#4CAF50',
                  padding: '4px 12px',
                  marginRight: 8,
                  borderRadius: 4,
                  cursor: 'pointer'
                }}
                onClick={handleAddConfirm}
              >
                Confirm
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
                Cancel
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
