import React from 'react';
import type { Position } from '../types/position';
import { POSITION_CONSTANTS } from '../constants/positionConstants';
import './PositionsTable.css';

interface PositionFormProps {
  position: Position;
  isSubmitting: boolean;
  error: string | null;
  onInputChange: (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => void;
  onSubmit: () => void;
  onCancel: () => void;
  isEdit: boolean;
}

const PositionForm: React.FC<PositionFormProps> = React.memo(({
  position,
  isSubmitting,
  error,
  onInputChange,
  onSubmit,
  onCancel,
  isEdit
}) => {
  const formId = isEdit ? 'edit-position-form' : 'add-position-form';
  const submitButtonText = isSubmitting ? 'Saving...' : (isEdit ? '[Update]' : 'Add');

  return (
    <div className="position-form" style={POSITION_CONSTANTS.STYLES.FORM_CONTAINER}>
      <form id={formId} onSubmit={(e) => { e.preventDefault(); onSubmit(); }}>
        <div>
          <input
            id="title"
            name="title"
            placeholder="Title*"
            maxLength={POSITION_CONSTANTS.VALIDATION.MAX_TITLE_LENGTH}
            value={position.title}
            onChange={onInputChange}
            aria-label="Position title"
            aria-required="true"
            required
          />
          <input
            id="description"
            name="description"
            placeholder="Description*"
            maxLength={POSITION_CONSTANTS.VALIDATION.MAX_DESCRIPTION_LENGTH}
            value={position.description}
            onChange={onInputChange}
            aria-label="Position description"
            aria-required="true"
            required
          />
          <input
            id="location"
            name="location"
            placeholder="Location*"
            value={position.location}
            onChange={onInputChange}
            aria-label="Position location"
            aria-required="true"
            required
          />
          <select
            id="status"
            name="status"
            value={position.status}
            onChange={onInputChange}
            aria-label="Position status"
          >
            <option value={POSITION_CONSTANTS.STATUS.DRAFT}>{POSITION_CONSTANTS.STATUS.DRAFT}</option>
            <option value={POSITION_CONSTANTS.STATUS.OPEN}>{POSITION_CONSTANTS.STATUS.OPEN}</option>
            <option value={POSITION_CONSTANTS.STATUS.CLOSED}>{POSITION_CONSTANTS.STATUS.CLOSED}</option>
            <option value={POSITION_CONSTANTS.STATUS.ARCHIVED}>{POSITION_CONSTANTS.STATUS.ARCHIVED}</option>
          </select>
          <input
            id="recruiterId"
            name="recruiterId"
            placeholder="RecruiterId*"
            value={position.recruiterId}
            onChange={onInputChange}
            aria-label="Recruiter ID"
            aria-required="true"
            required
          />
          <input
            id="departmentId"
            name="departmentId"
            placeholder="DepartmentId*"
            value={position.departmentId}
            onChange={onInputChange}
            aria-label="Department ID"
            aria-required="true"
            required
          />
          <input
            id="budget"
            name="budget"
            type="number"
            placeholder="Budget*"
            value={position.budget}
            onChange={onInputChange}
            aria-label="Position budget"
            aria-required="true"
            required
          />
          <input
            id="closingDate"
            name="closingDate"
            type="date"
            placeholder="Closing date"
            value={position.closingDate || ''}
            onChange={onInputChange}
            aria-label="Position closing date"
          />
          <button
            type="submit"
            className="action-button"
            disabled={isSubmitting}
            aria-label={submitButtonText}
          >
            {submitButtonText}
          </button>
          <button
            type="button"
            className="delete-button"
            onClick={onCancel}
            aria-label="Cancel form"
          >
            [Cancel]
          </button>
        </div>
      </form>
      {error && (
        <div className="error-message" role="alert" aria-live="polite">
          {error}
        </div>
      )}
    </div>
  );
});

PositionForm.displayName = 'PositionForm';

export default PositionForm; 