import React from 'react';
import type { Position } from '../types/position';
// import { POSITION_CONSTANTS } from '../constants/positionConstants';
import './PositionsTable.css';

interface PositionRowProps {
  position: Position;
  index: number;
  isExpanded: boolean;
  isDeleting: boolean;
  onExpand: (idx: number) => void;
  onEdit: (position: Position, index: number) => void;
  onDelete: (id: string, title: string) => Promise<void>;
}

const PositionRow: React.FC<PositionRowProps> = React.memo(({
  position,
  index,
  isExpanded,
  isDeleting,
  onExpand,
  onEdit,
  onDelete
}) => {
  const handleRowClick = (e: React.MouseEvent) => {
    const target = e.target as HTMLElement;
    if (target.tagName !== 'BUTTON' && !target.closest('button')) {
      onExpand(index);
    }
  };

  const handleEditClick = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    onEdit(position, index);
  };

  const handleDeleteClick = async (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    
    const positionId = position.positionId || position.id;
    if (!positionId || isDeleting) {
      console.error('Cannot delete: Missing position ID or already deleting', { position });
      return;
    }
    
    await onDelete(positionId.toString(), position.title);
  };

  const positionId = position.id || `position-${index}`;
  const isDeletingThis = isDeleting;

  return (
    <React.Fragment>
      <tr 
        onClick={handleRowClick} 
        style={{ cursor: 'pointer' }}
        tabIndex={0}
        role="button"
        aria-expanded={isExpanded}
        aria-label={`Position: ${position.title}. Click to ${isExpanded ? 'collapse' : 'expand'} details`}
        onKeyDown={(e) => {
          if (e.key === 'Enter' || e.key === ' ') {
            e.preventDefault();
            onExpand(index);
          }
        }}
      >
        <td
          style={{ color: '#1976d2' }}
          title="Click to show details"
        >
          {position.title}
        </td>
        <td>{position.description}</td>
        <td style={{ textAlign: 'center', whiteSpace: 'nowrap' }}>
          <button
            className="action-button"
            onClick={handleEditClick}
            aria-label={`Edit position: ${position.title}`}
            title={`Edit position: ${position.title}`}
          >
            [Edit]
          </button>
          <button 
            className="delete-button"
            onClick={handleDeleteClick}
            disabled={!position.positionId && !position.id || isDeletingThis}
            aria-label={`Delete position: ${position.title}`}
            title={`Delete position: ${position.title}`}
          >
            [Delete]
          </button>
        </td>
      </tr>
      {isExpanded && (
        <tr key={`expanded-${positionId}`}>
          <td colSpan={3} style={{ background: '#c0c0c0', color: '#333' }}>
            <div><strong>Location:</strong> {position.location}</div>
            <div><strong>Status:</strong> {position.status}</div>
            <div><strong>RecruiterId:</strong> {position.recruiterId}</div>
            <div><strong>DepartmentId:</strong> {position.departmentId}</div>
            <div><strong>Budget:</strong> {position.budget}</div>
            <div><strong>Closing date:</strong> {position.closingDate || '-'}</div>
          </td>
        </tr>
      )}
    </React.Fragment>
  );
});

PositionRow.displayName = 'PositionRow';

export default PositionRow; 