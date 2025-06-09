import type { RowData } from "@/types/RowData";
import toLabelFromProperty from "@/utils/toLabelFromProperty";
import { Modal } from "react-bootstrap";

interface RowDetailsModalProps {
    show: boolean;
    onHide: () => void;
    row: RowData | null;
}

export default function RowDetailsModal({ show, onHide, row }: RowDetailsModalProps) {
    return (
        <Modal show={show} onHide={onHide} centered>
            <Modal.Header closeButton>
                <Modal.Title>Details</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                {row && (
                    <ul className="list-unstyled mb-0">
                        {Object.entries(row).map(([key, value]) => (
                            <li key={key}>
                                <strong>{toLabelFromProperty(key)}:</strong> {value as React.ReactNode}
                            </li>
                        ))}
                    </ul>
                )}
            </Modal.Body>
        </Modal>
    );
}