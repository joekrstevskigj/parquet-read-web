import PaginationControl from "@/components/MainContent/PaginationControl";
import RowDetailsModal from "@/components/MainContent/RowDetailsModal";
import TableViewData from "@/components/MainContent/TableViewData";
import type { RowData } from "@/types/RowData";
import { useEffect, useState } from "react";
import type { EmployeesResponse } from "@/types/EmployeesResponse";
import type { EmployeesRequest } from "@/types/EmployeesRequest";
import FullPageSpinner from "@/components/FullPageSpinner";

const alwaysShow = [
    "firstName",
    "lastName",
    "title",
    "country",
    "registrationDate"
];

export default function MainContentContainer(
    {
        loading,
        error,
        employeesResponse,
        setRequest
    }: {
        loading: boolean,
        error: string | null,
        employeesResponse: EmployeesResponse | null,
        setRequest: React.Dispatch<React.SetStateAction<EmployeesRequest>>
    }) {
    const [showModal, setShowModal] = useState(false);
    const [modalData, setModalData] = useState<RowData | null>(null);
    const [currentPage, setCurrentPage] = useState(1);
    const [pageSize, setPageSize] = useState(50);

    useEffect(() => {
        setRequest(prev => ({
            ...prev,
            Page: currentPage,
            PageSize: pageSize
        }));
    }, [currentPage, pageSize]);

    const handleShowDetails = (row: RowData) => {
        setModalData(row);
        setShowModal(true);
    };

    // Pagination logic
    const data = employeesResponse?.employees || [];
    const totalRecordCount = employeesResponse?.totalRecordCount || 0;
    const totalPages = Math.ceil(totalRecordCount / pageSize);

    return (
        <>
            <FullPageSpinner show={loading} />

            {error && <h1 className="text-danger mt-5 pt-5">No data found.</h1>}

            {!loading && !error && (
                <>
                    <TableViewData
                        data={data}
                        alwaysShow={alwaysShow}
                        onShowDetails={handleShowDetails}
                    />
                    <PaginationControl
                        currentPage={currentPage}
                        totalPages={totalPages}
                        pageSize={pageSize}
                        onPageChange={setCurrentPage}
                        onPageSizeChange={size => {
                            setPageSize(size);
                            setCurrentPage(1);
                        }}
                    />
                </>
            )}
            <RowDetailsModal show={showModal} onHide={() => setShowModal(false)} row={modalData} />
        </>
    );
}