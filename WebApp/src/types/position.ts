import type { Status } from '../constants/positionConstants';

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

export interface PositionTableProps {
  positions: Position[];
  expandedIndex: number | null;
  onExpand: (idx: number) => void;
  onDelete: (id: string) => void;
  onUpdate: (id: string, position: Position) => void;
  onAdd?: (position: Position) => void;
} 