'use client';
import React, { useCallback, useEffect, useState } from 'react';
import DataTable from 'react-data-table-component';
import { Button, IconButton, Menu, MenuHandler, MenuItem, MenuList } from '@material-tailwind/react';
import { StreamEntity, StreamPaginatedResponse } from '@/backend/dto/stream.dto';
import Swal from 'sweetalert2';
import { SkeletonLoader } from '@/components/PlaceHolders/SkeletonLoader';
import { CreateEditStreamModal } from '@/components/Modals/CreateEditStreamModal';
import { deleteStream, getStreams } from '@/backend/stream';

export const StreamsWrapper = () => {
    const [loading, setLoading] = useState(false);
    const [response, setResponse] = useState<StreamPaginatedResponse | undefined>(undefined);
    const [selectedStream, setSelectedStream] = useState<StreamEntity | undefined>(undefined);
    const [editStream, setEditStream] = useState<boolean>(false);
    const [createStream, setCreateStream] = useState<boolean>(false);
    const [page, setPage] = useState(1);
    const [perPage, setPerPage] = useState(20);

    const loadStreams = useCallback(
        (inputPage: number = 0, inputSize: number = 20) => {
            setLoading(true);
            getStreams(inputPage, inputSize)
                .then((r) => {
                    if (r) setResponse(r);
                })
                .finally(() => setLoading(false));
        },
        [page, perPage]
    );

    useEffect(() => {
        loadStreams(page, perPage);
    }, [page, perPage, loadStreams]);

    const deleteConfirm = async (id: string) => {
        const result = await Swal.fire({
            text: 'Do you want to delete the stream item?',
            showDenyButton: false,
            showCancelButton: true,
            confirmButtonColor: '#ed2323',
            confirmButtonText: 'Yes, delete it!',
            cancelButtonColor: '#5a5a5a',
            cancelButtonText: 'Cancel',
            icon: 'question',
        });

        if (result.isConfirmed) {
            //something
            const delResponse = await deleteStream(id);
            if (delResponse) {
                await Swal.fire({
                    text: 'Stream successfully deleted!',
                    icon: 'success',
                    timer: 2000,
                    confirmButtonColor: '#5750F1',
                    confirmButtonText: 'OK',
                });
            }
            loadStreams();
        }
    };

    const columns = [
        {
            name: 'Id',
            cell: (row: any) => {
                return <span>{row.streamId}</span>;
            },
        },
        {
            name: 'Url',
            width: '200px',
            cell: (row: any) => {
                return (
                    <a href={row.url} target="_blank">
                        {row.url}
                    </a>
                );
            },
        },
        {
            name: 'Start time - End time',
            cell: (row: any) => {
                return (
                    <div className="py-7">
                        <p className="mb-0">
                            Start time: <b>{new Date(row.startTime).toLocaleString()}</b>
                        </p>
                        {row.endTime && (
                            <p className="mb-0">
                                Start time: <b>{new Date(row.startTime).toLocaleString()}</b>
                            </p>
                        )}
                    </div>
                );
            },
        },
        {
            name: 'Actions',
            cell: (row: any) => {
                const stream = row as StreamEntity;
                return (
                    <Menu
                        animate={{
                            mount: { y: 0 },
                            unmount: { y: 25 },
                        }}
                    >
                        <MenuHandler>
                            <IconButton variant="text">
                                <span className="font-semibold">...</span>
                            </IconButton>
                        </MenuHandler>
                        <MenuList>
                            <MenuItem
                                onClick={() => {
                                    setSelectedStream(stream);
                                    setEditStream(true);
                                }}
                            >
                                Edit
                            </MenuItem>
                            <MenuItem onClick={() => deleteConfirm(stream.streamId)}>Delete</MenuItem>
                        </MenuList>
                    </Menu>
                );
            },
        },
    ];

    return (
        <div className="rounded-[10px] border border-stroke bg-white p-4 shadow-1 dark:border-dark-3 dark:bg-gray-dark dark:shadow-card sm:p-7.5">
            <div style={{ display: 'flex', justifyContent: 'space-between' }} className="mb-6">
                <div style={{ display: 'flex', alignItems: 'center' }}></div>
                <div>
                    <Button onClick={() => setCreateStream(true)} color="green">
                        Create stream
                    </Button>
                </div>
            </div>
            <hr />
            <DataTable
                columns={columns}
                data={response?.data || []}
                progressPending={loading}
                progressComponent={<SkeletonLoader />}
                pagination
                paginationServer
                paginationTotalRows={response?.total || 0}
                paginationPerPage={perPage}
                onChangePage={(page) => setPage(page)}
                onChangeRowsPerPage={(perPage) => setPerPage(perPage)}
                paginationComponentOptions={{
                    rowsPerPageText: 'Item per page',
                    rangeSeparatorText: 'in',
                }}
            />
            {createStream && (
                <CreateEditStreamModal
                    isOpen={createStream}
                    onClose={() => setCreateStream(false)}
                    callback={() => {
                        setCreateStream(false);
                        loadStreams();
                    }}
                />
            )}
            {editStream && selectedStream && (
                <CreateEditStreamModal
                    isOpen={editStream}
                    onClose={() => setEditStream(false)}
                    callback={() => {
                        setEditStream(false);
                        loadStreams();
                    }}
                    stream={selectedStream}
                />
            )}
        </div>
    );
};
