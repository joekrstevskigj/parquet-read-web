import { Pagination, Form } from "react-bootstrap";

interface PaginationControlProps {
    currentPage: number;
    totalPages: number;
    pageSize: number;
    onPageChange: (page: number) => void;
    onPageSizeChange: (size: number) => void;
}

const PAGE_SIZE_OPTIONS = [10, 50, 100, 500];

function getPageNumbers(currentPage: number, totalPages: number, maxPages = 5): number[] {
    if (totalPages <= maxPages) {
        return Array.from({ length: totalPages }, (_, i) => i + 1);
    }
    let start = Math.max(currentPage - 2, 1);
    let end = start + maxPages - 1;

    if (end > totalPages) {
        end = totalPages;
        start = Math.max(end - maxPages + 1, 1);
    }
    return Array.from({ length: end - start + 1 }, (_, i) => start + i);
}

export default function PaginationControl({
    currentPage,
    totalPages,
    pageSize,
    onPageChange,
    onPageSizeChange,
}: PaginationControlProps) {
    const pageNumbers = getPageNumbers(currentPage, totalPages, 5);

    return (
        <div
            className="d-flex justify-content-between align-items-center py-2 px-3"
            style={{
                position: "fixed",
                left: 0,
                right: 0,
                bottom: 0,
                background: "#fff",
                borderTop: "1px solid #dee2e6",
                zIndex: 1050,
            }}
        >
            <Form.Group className="mb-0 d-flex align-items-center" controlId="pageSizeSelect">
                <Form.Label className="mb-0 me-2" style={{ whiteSpace: "nowrap" }}>
                    Page size:
                </Form.Label>
                <Form.Select
                    size="sm"
                    value={pageSize}
                    onChange={e => onPageSizeChange(Number(e.target.value))}
                    style={{ width: 90 }}
                >
                    {PAGE_SIZE_OPTIONS.map(size => (
                        <option key={size} value={size}>
                            {size}
                        </option>
                    ))}
                </Form.Select>
            </Form.Group>
            <Pagination className="mb-0">
                <Pagination.First
                    onClick={() => onPageChange(1)}
                    disabled={currentPage === 1}
                />

                <Pagination.Prev
                    onClick={() => onPageChange(Math.max(1, currentPage - 1))}
                    disabled={currentPage === 1}
                />
                {pageNumbers.map(page => (
                    <Pagination.Item
                        key={page}
                        active={currentPage === page}
                        onClick={() => onPageChange(page)}
                    >
                        {page}
                    </Pagination.Item>
                ))}

                <Pagination.Next
                    onClick={
                        () => onPageChange(Math.min(totalPages, currentPage + 1))
                    }
                    disabled={currentPage === totalPages}
                />

                <Pagination.Last
                    onClick={() => onPageChange(totalPages)}
                    disabled={currentPage === totalPages}
                />
            </Pagination>
        </div>
    );
}